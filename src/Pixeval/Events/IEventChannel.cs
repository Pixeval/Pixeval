using System;

namespace Pixeval.Events
{
    /// <summary>
    /// An EventChannel is a model where there is a event publisher and multiple subscribers,
    /// the subscriber can subscribe to a particular event type with an event handler, the event
    /// handler will be called once the corresponding event is published into the channel
    /// </summary>
    public interface IEventChannel
    {
        void Subscribe<T>(Action<T> eventHandler) where T : IEvent;

        void Publish<T>(T eventObj) where T : IEvent;
    }
}