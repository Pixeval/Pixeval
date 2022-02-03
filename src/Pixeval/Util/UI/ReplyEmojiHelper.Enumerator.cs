#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/ReplyEmojiHelper.Enumerator.cs
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
using System.Linq;
using System.Text;
using Pixeval.Misc;


namespace Pixeval.Util.UI;

public static partial class ReplyEmojiHelper
{
    public static IEnumerable<ReplyContentToken> EnumerateTokens(string content)
    {
        var table = BuildEmojiReplacementIndexTableOfReplyContent(content);
        if (!table.Any())
        {
            yield return new ReplyContentToken.TextToken(content);
            yield break;
        }

        var stringBuilder = new StringBuilder();
        for (var i = 0; i < content.Length;)
        {
            while (!table.Keys.Contains(i) && i < content.Length)
            {
                stringBuilder.Append(content[i++]);
            }

            if (stringBuilder.Length != 0)
            {
                yield return new ReplyContentToken.TextToken(stringBuilder.ToString());
            }

            if (i >= content.Length)
            {
                yield break;
            }

            stringBuilder.Clear();
            var (emoji, length) = table[i];
            yield return new ReplyContentToken.EmojiToken(emoji);
            i += length;
        }
    }
}