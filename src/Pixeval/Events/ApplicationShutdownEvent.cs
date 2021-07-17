using Microsoft.UI.Xaml;

namespace Pixeval.Events
{
    /// <summary>
    /// This event will be published if the application encounters an unrecoverable error.
    /// See <see cref="App()"/>
    /// </summary>
    public class ApplicationShutdownAbnormallyEvent : IEvent
    {
        public object? Sender => Application.Current;
    }
}