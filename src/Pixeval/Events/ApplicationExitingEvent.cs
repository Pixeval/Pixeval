using Microsoft.UI.Xaml;

namespace Pixeval.Events
{
    /// <summary>
    /// This event will be published if the application is going to shutdown programmatically.
    /// See <see cref="App()"/>
    /// </summary>
    public class ApplicationExitingEvent : IEvent
    {
        public object? Parameter => Application.Current;
    }
}