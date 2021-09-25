using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pixeval.Misc;

namespace Pixeval.Util.UI
{
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
}
