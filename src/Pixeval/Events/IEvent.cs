namespace Pixeval.Events
{
    public interface IEvent
    {
        object? Parameter { get; }
    }
}