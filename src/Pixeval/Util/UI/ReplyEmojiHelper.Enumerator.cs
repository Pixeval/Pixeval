// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

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
