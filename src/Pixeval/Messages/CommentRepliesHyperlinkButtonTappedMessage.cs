namespace Pixeval.Messages
{
    public class CommentRepliesHyperlinkButtonTappedMessage
    {
        public CommentRepliesHyperlinkButtonTappedMessage(object? sender)
        {
            Sender = sender;
        }

        public object? Sender { get; }
    }
}