// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Frozen;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pixeval.Util.UI;

public static partial class ReplyEmojiHelper
{
    private static readonly FrozenDictionary<string, PixivReplyEmoji> _StringToEmojiTable = Enum.GetValues<PixivReplyEmoji>().ToFrozenDictionary(emoji => emoji.GetReplyEmojiPlaceholderKey());

    private static readonly string _Pattern = string.Join("|", _StringToEmojiTable.Keys.Select(Regex.Escape));

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
        return $"https://s.pximg.net/common/images/emoji/{(int) emoji}.png";
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

    public static string GetMarkdownUrlFromPlaceholderKey(this string content)
    {
        // #24表示显示的图片大小
        return $" ![{content}]({GetReplyEmojiDownloadUrl(_StringToEmojiTable[content])}#24) ";
    }

    public static string GetContents(string content)
    {
        var table = Regex.Matches(content, _Pattern);
        // var table = BuildEmojiReplacementIndexTableOfReplyContent(content);
        if (table.Count is 0)
            return content;

        var span = content.AsSpan();
        var sb = new StringBuilder();

        var lastEnd = 0;
        foreach (Match match in table)
        {
            if (match.Index > lastEnd)
                _ = sb.Append(span[lastEnd..match.Index]);
            lastEnd = match.Index + match.Length;
            sb.Append(GetMarkdownUrlFromPlaceholderKey(span[match.Index..lastEnd].ToString()));
        }

        if (span.Length > lastEnd)
            sb.Append(span[lastEnd..]);

        return sb.ToString();
    }
}
