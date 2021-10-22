#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/CommentInline.cs
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
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Enums;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Helpers;

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Inlines
{
    /// <summary>
    ///     Represents a span that contains comment.
    /// </summary>
    internal class CommentInline : MarkdownInline, IInlineLeaf
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CommentInline" /> class.
        /// </summary>
        public CommentInline()
            : base(MarkdownInlineType.Comment)
        {
        }

        /// <summary>
        ///     Gets or sets the Content of the Comment.
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        ///     Returns the chars that if found means we might have a match.
        /// </summary>
        internal static void AddTripChars(List<InlineTripCharHelper> tripCharHelpers)
        {
            tripCharHelpers.Add(new InlineTripCharHelper { FirstChar = '<', Method = InlineParseMethod.Comment });
        }

        /// <summary>
        ///     Attempts to parse a comment span.
        /// </summary>
        /// <param name="markdown"> The markdown text. </param>
        /// <param name="start"> The location to start parsing. </param>
        /// <param name="maxEnd"> The location to stop parsing. </param>
        /// <returns> A parsed bold text span, or <c>null</c> if this is not a bold text span. </returns>
        internal static InlineParseResult? Parse(string markdown, int start, int maxEnd)
        {
            if (start >= maxEnd - 1)
            {
                return null;
            }

            var startSequence = markdown.Substring(start);
            if (!startSequence.StartsWith("<!--"))
            {
                return null;
            }

            // Find the end of the span.  The end sequence ('-->')
            var innerStart = start + 4;
            var innerEnd = Common.IndexOf(markdown, "-->", innerStart, maxEnd);
            if (innerEnd == -1)
            {
                return null;
            }

            var length = innerEnd - innerStart;
            var contents = markdown.Substring(innerStart, length);

            var result = new CommentInline
            {
                Text = contents
            };

            return new InlineParseResult(result, start, innerEnd + 3);
        }

        /// <summary>
        ///     Converts the object into it's textual representation.
        /// </summary>
        /// <returns> The textual representation of this object. </returns>
        public override string ToString()
        {
            return "<!--" + Text + "-->";
        }
    }
}