namespace Pixeval.Events
{
    public interface IEvent
    {
        object? Sender { get; }
    }
}