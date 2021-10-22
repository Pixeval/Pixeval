#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/CodeInline.cs
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
    ///     Represents a span containing code, or other text that is to be displayed using a
    ///     fixed-width font.
    /// </summary>
    public class CodeInline : MarkdownInline, IInlineLeaf
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeInline" /> class.
        /// </summary>
        public CodeInline()
            : base(MarkdownInlineType.Code)
        {
        }

        /// <summary>
        ///     Gets or sets the text to display as code.
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        ///     Returns the chars that if found means we might have a match.
        /// </summary>
        internal static void AddTripChars(List<InlineTripCharHelper> tripCharHelpers)
        {
            tripCharHelpers.Add(new InlineTripCharHelper { FirstChar = '`', Method = InlineParseMethod.Code });
        }

        /// <summary>
        ///     Attempts to parse an inline code span.
        /// </summary>
        /// <param name="markdown"> The markdown text. </param>
        /// <param name="start"> The location to start parsing. </param>
        /// <param name="maxEnd"> The location to stop parsing. </param>
        /// <returns> A parsed inline code span, or <c>null</c> if this is not an inline code span. </returns>
        internal static InlineParseResult? Parse(string markdown, int start, int maxEnd)
        {
            // Check the first char.
            if (start == maxEnd || markdown[start] != '`')
            {
                return null;
            }

            // There is an alternate syntax that starts and ends with two backtick.
            // e.g. ``sdf`sdf`` would be "sdf`sdf".
            var innerStart = start + 1;
            int innerEnd, end;
            if (innerStart < maxEnd && markdown[innerStart] == '`')
            {
                // Alternate double back-tick syntax.
                innerStart++;

                // Find the end of the span.
                innerEnd = Common.IndexOf(markdown, "``", innerStart, maxEnd);
                if (innerEnd == -1)
                {
                    return null;
                }

                end = innerEnd + 2;
            }
            else
            {
                // Standard single backtick syntax.

                // Find the end of the span.
                innerEnd = Common.IndexOf(markdown, '`', innerStart, maxEnd);
                if (innerEnd == -1)
                {
                    return null;
                }

                end = innerEnd + 1;
            }

            // The span must contain at least one character.
            if (innerStart == innerEnd)
            {
                return null;
            }

            // We found something!
            var result = new CodeInline
            {
                Text = markdown.Substring(innerStart, innerEnd - innerStart).Trim(' ', '\t', '\r', '\n')
            };
            return new InlineParseResult(result, start, end);
        }

        /// <summary>
        ///     Converts the object into it's textual representation.
        /// </summary>
        /// <returns> The textual representation of this object. </returns>
        public override string? ToString()
        {
            if (Text == null)
            {
                return base.ToString();
            }

            return "`" + Text + "`";
        }
    }
}