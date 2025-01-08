// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Util.UI;

namespace Pixeval.Util;

public abstract record ReplyContentToken
{
    public record TextToken(string Content) : ReplyContentToken;

    public record EmojiToken(PixivReplyEmoji Emoji) : ReplyContentToken;
}
