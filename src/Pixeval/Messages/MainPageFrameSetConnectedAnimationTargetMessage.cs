using Microsoft.UI.Xaml;

namespace Pixeval.Messages
{
    /// <summary>
    /// This event occurs when the target of the connected animation between MainPage
    /// and the other one page is selected
    /// </summary>
    public record MainPageFrameSetConnectedAnimationTargetMessage(UIElement? Sender);
}