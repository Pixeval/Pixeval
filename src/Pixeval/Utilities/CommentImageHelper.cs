// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pixeval.Utilities;

public static class CommentImageHelper
{
    public static IReadOnlyList<int> StickerIds { get; } =
    [
        .. Enumerable.Range(301, 10),
        .. Enumerable.Range(401, 10),
        .. Enumerable.Range(201, 10),
        .. Enumerable.Range(101, 10)
    ];

    public static IReadOnlyList<int> EmojiIds { get; } = (int[]) Enum.GetValuesAsUnderlyingType<PixivReplyEmoji>();


    public const string StickerDownloadUrl = "https://s.pximg.net/common/images/stamp/generated-stamps/{0}_s.jpg";


    public const string EmojiDownloadUrl = "https://s.pximg.net/common/images/emoji/{0}.png";


    private static readonly FrozenDictionary<string, PixivReplyEmoji> _StringToEmojiTable = Enum.GetValues<PixivReplyEmoji>().ToFrozenDictionary(emoji => emoji.PlaceholderKey);

    private static readonly string _Pattern = string.Join("|", _StringToEmojiTable.Keys.Select(Regex.Escape));

    extension(PixivReplyEmoji emoji)
    {
        /// <summary>
        /// Returns the placeholder of the emoji in the reply content which has a form of
        /// "(emoji_name)"
        /// </summary>
        public string PlaceholderKey=> $"({emoji.ToString().ToLowerInvariant()})";

        /// <summary>
        /// Returns the url of the png image of the <see cref="PixivReplyEmoji" />
        /// </summary>
        public string DownloadUrl => string.Format(EmojiDownloadUrl, emoji.ToString("D"));
    }

    public static string GetMarkdownUrlFromPlaceholderKey(this string content)
    {
        // #24表示显示的图片大小
        return $" ![{content}]({_StringToEmojiTable[content].DownloadUrl}#24) ";
    }

    public static string GetContents(string content)
    {
        var table = Regex.Matches(content, _Pattern);
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
            sb.Append(span[match.Index..lastEnd].ToString().GetMarkdownUrlFromPlaceholderKey());
        }

        if (span.Length > lastEnd)
            sb.Append(span[lastEnd..]);

        return sb.ToString();
    }
}
