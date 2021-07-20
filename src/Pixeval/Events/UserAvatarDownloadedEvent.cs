namespace Pixeval.Events
{
    /// <summary>
    /// This event will be published when the user avatar is completely downloaded.
    /// </summary>
    public class UserAvatarDownloadedEvent : IEvent
    {
        public UserAvatarDownloadedEvent(object sender)
        {
            Sender = sender;
        }
        
        public object? Sender { get; }
    }
}