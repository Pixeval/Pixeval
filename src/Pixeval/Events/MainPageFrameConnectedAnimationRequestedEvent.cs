namespace Pixeval.Events
{
    /// <summary>
    /// This event occurs when the target of the connected animation between MainPage
    /// and the other one page is selected
    /// </summary>
    public class MainPageFrameSetConnectedAnimationTargetEvent : IEvent
    {
        public MainPageFrameSetConnectedAnimationTargetEvent(object? sender)
        {
            Parameter = sender;
        }

        public object? Parameter { get; }
    }
}