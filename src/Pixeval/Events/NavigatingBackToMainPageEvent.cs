using Pixeval.Pages;
using Pixeval.Pages.IllustrationViewer;

namespace Pixeval.Events
{
    /// <summary>
    /// This event occurs when <see cref="IllustrationViewerPage"/> is about to navigate back to
    /// <see cref="MainPage"/>, the parameter contains the item that the <see cref="IllustrationViewerPage"/>
    /// is currently browsing
    /// </summary>
    public class NavigatingBackToMainPageEvent : IEvent
    {
        public NavigatingBackToMainPageEvent(object? parameter)
        {
            Parameter = parameter;
        }

        public object? Parameter { get; }
    }
}