namespace Pixeval.Events
{
    public interface IEvent
    {
        public object? Sender { get; }
    }
}