using Pixeval.Pages;

namespace Pixeval.Messages
{
    /// <summary>
    /// This event occurs when the MainPage's frame is navigating from one page to another
    /// </summary>
    public record MainPageFrameNavigatingEvent(MainPage Sender);
}