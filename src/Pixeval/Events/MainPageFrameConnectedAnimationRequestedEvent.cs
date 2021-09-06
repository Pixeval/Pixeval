namespace Pixeval.Events
{
    public class MainPageFrameConnectedAnimationRequestedEvent : IEvent
    {
        public MainPageFrameConnectedAnimationRequestedEvent(object? sender)
        {
            Sender = sender;
        }

        public object? Sender { get; }
    }
}