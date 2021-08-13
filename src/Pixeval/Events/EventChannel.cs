using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mako.Util;

namespace Pixeval.Events
{
    /// <summary>
    /// An uber-lightweight implementation of an event bus
    /// </summary>
    public class EventChannel : IEventChannel
    {
        static EventChannel()
        {
            Default = new EventChannel();
        }

        public static EventChannel Default;

        private readonly ConcurrentDictionary<Type, IList<Action<IEvent>>> _registeredSubscribers = new();

        public void Subscribe<T>(Action<T> eventHandler) where T : IEvent
        {
            GetOrCreateSubscriber<T>(evt => eventHandler((T) evt));
        }

        public void Subscribe<T>(Action eventHandler) where T : IEvent
        {
            GetOrCreateSubscriber<T>(_ => eventHandler());
        }

        public Task SubscribeAsync<T>(Func<T, Task> eventHandler) where T : IEvent
        {
            var tcs = new TaskCompletionSource();
            GetOrCreateSubscriber<T>(evt => eventHandler((T) evt).ContinueWith(_ => tcs.SetResult()));
            return tcs.Task;
        }

        private void GetOrCreateSubscriber<T>(Action<IEvent> handler) where T : IEvent
        {
            if (_registeredSubscribers.TryGetValue(typeof(T), out var list))
            {
                list.Add(handler);
                return;
            }

            _registeredSubscribers[typeof(T)] = new List<Action<IEvent>> {handler};
        }

        public void Publish<T>(T eventObj) where T : IEvent
        {
            if (_registeredSubscribers.TryGetValue(eventObj.GetType(), out var list))
            {
                list.ForEach(action => action(eventObj));
            }
        }
    }
}