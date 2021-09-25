using Microsoft.UI.Xaml;

namespace Pixeval.Messages
{
    /// <summary>
    /// This event occurs when the target of the connected animation between MainPage
    /// and the other one page is selected
    /// </summary>
    public class MainPageFrameSetConnectedAnimationTargetMessage
    {
        public MainPageFrameSetConnectedAnimationTargetMessage(UIElement? sender)
        {
            Parameter = sender;
        }

        public UIElement? Parameter { get; }
    }
}