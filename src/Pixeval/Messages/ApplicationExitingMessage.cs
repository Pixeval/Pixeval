using Microsoft.UI.Xaml;

namespace Pixeval.Messages
{
    /// <summary>
    /// This event will be published if the application is going to shutdown programmatically.
    /// See <see cref="App()"/>
    /// </summary>
    public class ApplicationExitingMessage
    {
        public Application Parameter => Application.Current;
    }
}