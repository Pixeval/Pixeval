// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Pixeval.Util.UI;

public static partial class ReplyEmojiHelper
{
    private static readonly IReadOnlyDictionary<string, PixivReplyEmoji> _StringToEmojiTable = BuildQueryTable();

    private static ImmutableDictionary<string, PixivReplyEmoji> BuildQueryTable()
    {
        return Enum.GetValues<PixivReplyEmoji>().ToImmutableDictionary(emoji => $"({emoji.ToString().ToLowerInvariant()})");
    }

    /// <summary>
    /// Returns the placeholder of the emoji in the reply content which has a form of
    /// "(emoji_name)"
    /// </summary>
    public static string GetReplyEmojiPlaceholderKey(this PixivReplyEmoji emoji)
    {
        return $"({emoji.ToString().ToLowerInvariant()})";
    }

    /// <summary>
    /// Returns the url of the png image of the <see cref="PixivReplyEmoji" />
    /// </summary>
    public static string GetReplyEmojiDownloadUrl(this PixivReplyEmoji emoji)
    {
        return $"https://s.pximg.net/common/images/emoji/{(int)emoji}.png";
    }

    public static PixivReplyEmoji GetReplyEmojiFromPlaceholderKey(string content)
    {
        return _StringToEmojiTable[content];
    }

    //public static IReadOnlyDictionary<int, (PixivReplyEmoji emoji, int contentLength)> BuildEmojiReplacementIndexTableOfReplyContent(string replyContent)
    //{
    //    return Regex.Matches(replyContent, string.Join("|", _stringToEmojiTable.Keys.Select(Regex.Escape)))
    //        .ToImmutableDictionary(m => m.Index, m => (_stringToEmojiTable[m.Value], m.Value.Length));
    //}
}
