using Pixeval.Pages;

namespace Pixeval.Messages
{
    /// <summary>
    /// This event occurs when the MainPage's frame is navigating from one page to another
    /// </summary>
    public class MainPageFrameNavigatingEvent
    {
        public MainPage Sender { get; }

        public MainPageFrameNavigatingEvent(MainPage sender)
        {
            Sender = sender;
        }
    }
}