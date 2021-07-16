using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Mako.Util;

namespace Pixeval.Events
{
    /// <summary>
    /// An uber-lightweight implementation of an event bus
    /// </summary>
    public class EventChannel : IEventChannel
    {
        public static EventChannel CreateStarted()
        {
            var eventChannel = new EventChannel();
            eventChannel.Start();
            return eventChannel;
        }

        public IAsyncEnumerable<T> OfType<T>() where T : IEvent
        {
            return this.Where(t => t is T).Select(t => (T) t);
        }

        private readonly ConcurrentDictionary<Type, IList<Action<IEvent>>> _registeredSubscribers = new();

        private readonly Channel<IEvent> _eventChannel = Channel.CreateUnbounded<IEvent>();

        public void Subscribe<T>(Action<T> eventHandler) where T : IEvent
        {
            void Wrapper(IEvent evt) => eventHandler((T) evt);
            if (_registeredSubscribers.TryGetValue(typeof(T), out var list))
            {
                list.Add(Wrapper);
            }

            _registeredSubscribers[typeof(T)] = new List<Action<IEvent>> {Wrapper};
        }

        /// <summary>
        /// Exposes the control-flow inside the callee to the caller by using <see cref="TaskCompletionSource"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventHandler"></param>
        /// <returns></returns>
        public Task SubscribeTask<T>(Action<T, TaskCompletionSource> eventHandler) where T : IEvent
        {
            var tcs = new TaskCompletionSource();
            void Wrapper(IEvent evt) => eventHandler((T) evt, tcs);
            if (_registeredSubscribers.TryGetValue(typeof(T), out var list))
            {
                list.Add(Wrapper);
            }

            _registeredSubscribers[typeof(T)] = new List<Action<IEvent>> { Wrapper };
            return tcs.Task;
        }


        public ValueTask Publish<T>(T eventObj) where T : IEvent
        {
            return _eventChannel.Writer.WriteAsync(eventObj);
        }

        public void Start()
        {
            // Start a event loop that runs at background
            Task.Run(async () =>
            {
                var reader = _eventChannel.Reader;
                while (!reader.Completion.IsCompleted)
                {
                    var evt = await reader.ReadAsync();
                    if (_registeredSubscribers.TryGetValue(evt.GetType(), out var list))
                    {
                        list.ForEach(action => action(evt));
                    }
                }
            });
        }

        public void Close()
        {
            _eventChannel.Writer.Complete();
        }

        public IAsyncEnumerator<IEvent> GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            return new EventChannelAsyncEnumerator(_eventChannel)!;
        }

        public class EventChannelAsyncEnumerator : IAsyncEnumerator<IEvent?>
        {
            private readonly Channel<IEvent> _eventChannel;

            private bool _hasNext = true;

            public EventChannelAsyncEnumerator(Channel<IEvent> eventChannel)
            {
                _eventChannel = eventChannel;
                _eventChannel.Reader.Completion.ContinueWith(_ => _hasNext = false);
            }

            public ValueTask DisposeAsync()
            {
                return ValueTask.CompletedTask;
            }

            public async ValueTask<bool> MoveNextAsync()
            {
                Current = await _eventChannel.Reader.ReadAsync();
                return _hasNext;
            }

            public IEvent? Current { get; private set; }
        }
    }
}