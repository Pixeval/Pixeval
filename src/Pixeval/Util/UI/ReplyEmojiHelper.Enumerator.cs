#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/ReplyEmojiHelper.Enumerator.cs
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pixeval.Util.UI;

public static partial class ReplyEmojiHelper
{
    public static IEnumerable<ReplyContentToken> EnumerateTokens(string content)
    {
        var table = Regex.Matches(content, string.Join("|", _StringToEmojiTable.Keys.Select(Regex.Escape)))
            .ToImmutableDictionary(m => m.Index, m => (_StringToEmojiTable[m.Value], m.Value.Length));
        // var table = BuildEmojiReplacementIndexTableOfReplyContent(content);
        if (table.Count is 0)
        {
            yield return new ReplyContentToken.TextToken(content);
            yield break;
        }

        var stringBuilder = new StringBuilder();
        for (var i = 0; i < content.Length;)
        {
            while (!table.ContainsKey(i) && i < content.Length)
            {
                _ = stringBuilder.Append(content[i++]);
            }

            if (stringBuilder.Length is not 0)
            {
                yield return new ReplyContentToken.TextToken(stringBuilder.ToString());
            }

            if (i >= content.Length)
            {
                yield break;
            }

            _ = stringBuilder.Clear();
            var (emoji, length) = table[i];
            yield return new ReplyContentToken.EmojiToken(emoji);
            i += length;
        }
    }
}
