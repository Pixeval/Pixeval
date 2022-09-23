#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/ReplyEmojiHelper.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Pixeval.Misc;


namespace Pixeval.Util.UI;

public static partial class ReplyEmojiHelper
{
    private static readonly IReadOnlyDictionary<string, PixivReplyEmoji> StringToEmojiTable = BuildQueryTable();

    private static IReadOnlyDictionary<string, PixivReplyEmoji> BuildQueryTable()
    {
        return Enum.GetValues<PixivReplyEmoji>().ToImmutableDictionary(emoji => $"({emoji.ToString().ToLowerInvariant()})");
    }

    /// <summary>
    ///     Returns the placeholder of the emoji in the reply content which has a form of
    ///     "(emoji_name)"
    /// </summary>
    public static string GetReplyEmojiPlaceholderKey(this PixivReplyEmoji emoji)
    {
        return $"({emoji.ToString().ToLowerInvariant()})";
    }

    /// <summary>
    ///     Returns the url of the png image of the <see cref="PixivReplyEmoji" />
    /// </summary>
    public static string GetReplyEmojiDownloadUrl(this PixivReplyEmoji emoji)
    {
        return $"https://s.pximg.net/common/images/emoji/{(int)emoji}.png";
    }

    public static PixivReplyEmoji GetReplyEmojiFromPlaceholderKey(string content)
    {
        return StringToEmojiTable[content];
    }

    public static IReadOnlyDictionary<int, (PixivReplyEmoji emoji, int contentLength)> BuildEmojiReplacementIndexTableOfReplyContent(string replyContent)
    {
        return Regex.Matches(replyContent, string.Join("|", StringToEmojiTable.Keys.Select(Regex.Escape)))
            .ToImmutableDictionary(m => m.Index, m => (StringToEmojiTable[m.Value], m.Value.Length));
    }
}