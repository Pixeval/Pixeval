using System;
using Microsoft.UI.Dispatching;

namespace Pixeval.Events
{
    public static class EventChannelExtensions
    {
        public static void SubscribeOn<T>(this EventChannel eventChannel, DispatcherQueue dispatcherQueue, Action<T> eventHandler) where T : IEvent
        {
            eventChannel.Subscribe<T>(t => dispatcherQueue.TryEnqueue(() => eventHandler(t)));
        }

        public static void SubscribeOn<T>(this EventChannel eventChannel, DispatcherQueue dispatcherQueue, Action eventHandler) where T : IEvent
        {
            eventChannel.Subscribe<T>(_ => dispatcherQueue.TryEnqueue(() => eventHandler()));
        }

        public static void SubscribeOnUIThread<T>(this EventChannel eventChannel, Action<T> eventHandler) where T : IEvent
        {
            eventChannel.Subscribe<T>(t => App.Window.DispatcherQueue.TryEnqueue(() => eventHandler(t)));
        }

        public static void SubscribeOnUIThread<T>(this EventChannel eventChannel, Action eventHandler) where T : IEvent
        {
            eventChannel.Subscribe<T>(_ => App.Window.DispatcherQueue.TryEnqueue(() => eventHandler()));
        }
    }
}