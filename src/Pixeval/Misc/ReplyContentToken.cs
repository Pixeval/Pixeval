namespace Pixeval.Misc
{
    public abstract record ReplyContentToken
    {
        public record TextToken(string Content) : ReplyContentToken;

        public record EmojiToken(PixivReplyEmoji Emoji) : ReplyContentToken;
    }
}