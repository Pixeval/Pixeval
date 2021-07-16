using Microsoft.UI.Xaml;

namespace Pixeval.Events
{
    public class ApplicationShutdownEvent : IEvent
    {
        public object? Sender => Application.Current;
    }
}