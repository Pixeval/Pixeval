#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/EmojiInline.cs
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
using Pixeval.CommunityToolkit.Markdown.Parsers.Core;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Enums;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Helpers;

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Inlines
{
    /// <summary>
    ///     Represents a span containing emoji symbol.
    /// </summary>
    public partial class EmojiInline : MarkdownInline, IInlineLeaf
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EmojiInline" /> class.
        /// </summary>
        public EmojiInline()
            : base(MarkdownInlineType.Emoji)
        {
        }

        /// <inheritdoc />
        public string? Text { get; set; }

        /// <summary>
        ///     Returns the chars that if found means we might have a match.
        /// </summary>
        internal static void AddTripChars(List<InlineTripCharHelper> tripCharHelpers)
        {
            tripCharHelpers.Add(new InlineTripCharHelper { FirstChar = ':', Method = InlineParseMethod.Emoji });
        }

        internal static InlineParseResult? Parse(string markdown, int start, int maxEnd)
        {
            if (start >= maxEnd - 1)
            {
                return null;
            }

            // Check the start sequence.
            var startSequence = markdown.Substring(start, 1);
            if (startSequence != ":")
            {
                return null;
            }

            // Find the end of the span.
            var innerStart = start + 1;
            var innerEnd = Common.IndexOf(markdown, startSequence, innerStart, maxEnd);
            if (innerEnd == -1)
            {
                return null;
            }

            // The span must contain at least one character.
            if (innerStart == innerEnd)
            {
                return null;
            }

            // The first character inside the span must NOT be a space.
            if (ParseHelpers.IsMarkdownWhiteSpace(markdown[innerStart]))
            {
                return null;
            }

            // The last character inside the span must NOT be a space.
            if (ParseHelpers.IsMarkdownWhiteSpace(markdown[innerEnd - 1]))
            {
                return null;
            }

            var emojiName = markdown.Substring(innerStart, innerEnd - innerStart);

            if (_emojiCodesDictionary.TryGetValue(emojiName, out var emojiCode))
            {
                var result = new EmojiInline { Text = char.ConvertFromUtf32(emojiCode), Type = MarkdownInlineType.Emoji };
                return new InlineParseResult(result, start, innerEnd + 1);
            }

            return null;
        }
    }
}