// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Pixeval.Controls;

namespace Pixeval.Util.UI;

public static partial class ReplyEmojiHelper
{
    private static readonly FrozenDictionary<string, PixivReplyEmoji> _StringToEmojiTable = Enum.GetValues<PixivReplyEmoji>().ToFrozenDictionary(emoji => emoji.GetReplyEmojiPlaceholderKey());

    private static string _Pattern = string.Join("|", _StringToEmojiTable.Keys.Select(Regex.Escape));

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

    /// <summary>
    /// Returns the url of the png image of the <see cref="PixivReplyEmoji" />
    /// </summary>
    public static string GetReplyEmojiDownloadUrlFromPlaceholderKey(this string content)
    {
        return GetReplyEmojiDownloadUrl(_StringToEmojiTable[content]);
    }

    public static PixivReplyEmoji GetReplyEmojiFromPlaceholderKey(string content)
    {
        return _StringToEmojiTable[content];
    }

    public static IEnumerable<InlineContent> GetContents(string content)
    {
        var table = Regex.Matches(content, _Pattern);
        // var table = BuildEmojiReplacementIndexTableOfReplyContent(content);
        var tokens = new List<InlineContent>();
        if (table.Count is 0)
        {
            tokens.Add(new RunContent(content));
            return tokens;
        }

        var span = content.AsSpan();

        var lastEnd = 0;
        foreach (Match match in table)
        {
            if (match.Index > lastEnd)
                tokens.Add(new RunContent(span[lastEnd..match.Index].ToString()));
            lastEnd = match.Index + match.Length;
            tokens.Add(new ImageContent(GetReplyEmojiDownloadUrlFromPlaceholderKey(span[match.Index..lastEnd].ToString())));
        }

        if (span.Length > lastEnd)
            tokens.Add(new RunContent(span[lastEnd..].ToString()));

        return tokens;
    }
}
