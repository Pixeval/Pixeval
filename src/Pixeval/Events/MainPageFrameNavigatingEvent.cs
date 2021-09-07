using Microsoft.UI.Xaml;

namespace Pixeval.Events
{
    /// <summary>
    /// This event occurs when the MainPage's frame is navigating from one page to another
    /// </summary>
    public class MainPageFrameNavigatingEvent : IEvent
    {
        public object? Parameter { get; }

        public MainPageFrameNavigatingEvent(UIElement? sender)
        {
            Parameter = sender;
        }
    }
}