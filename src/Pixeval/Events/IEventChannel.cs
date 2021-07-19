using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixeval.Events
{
    /// <summary>
    /// An EventChannel is a model where there is a event publisher and multiple subscribers,
    /// the subscriber can subscribe to a particular event type with an event handler, the event
    /// handler will be called once the corresponding event is published into the channel
    /// </summary>
    public interface IEventChannel : IAsyncEnumerable<IEvent>
    {
        void Subscribe<T>(Action<T> eventHandler) where T : IEvent;

        ValueTask PublishAsync<T>(T eventObj) where T : IEvent;

        void Start();

        void Close();
    }
}