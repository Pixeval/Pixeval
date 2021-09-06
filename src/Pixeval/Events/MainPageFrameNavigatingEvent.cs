using Microsoft.UI.Xaml;
using Pixeval.Pages;

namespace Pixeval.Events
{
    public class MainPageFrameNavigatingEvent : IEvent
    {
        public object? Sender { get; }

        public MainPageFrameNavigatingEvent(UIElement? sender)
        {
            Sender = sender;
        }
    }
}