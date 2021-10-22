#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/HorizontalRuleBlock.cs
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

using Pixeval.CommunityToolkit.Markdown.Parsers.Core;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Enums;

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Blocks
{
    /// <summary>
    ///     Represents a horizontal line.
    /// </summary>
    public class HorizontalRuleBlock : MarkdownBlock
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HorizontalRuleBlock" /> class.
        /// </summary>
        public HorizontalRuleBlock()
            : base(MarkdownBlockType.HorizontalRule)
        {
        }

        /// <summary>
        ///     Parses a horizontal rule.
        /// </summary>
        /// <param name="markdown"> The markdown text. </param>
        /// <param name="start"> The location of the start of the line. </param>
        /// <param name="end"> The location of the end of the line. </param>
        /// <returns> A parsed horizontal rule block, or <c>null</c> if this is not a horizontal rule. </returns>
        internal static HorizontalRuleBlock? Parse(string markdown, int start, int end)
        {
            // A horizontal rule is a line with at least 3 stars, optionally separated by spaces
            // OR a line with at least 3 dashes, optionally separated by spaces
            // OR a line with at least 3 underscores, optionally separated by spaces.
            var hrChar = '\0';
            var hrCharCount = 0;
            var pos = start;
            while (pos < end)
            {
                var c = markdown[pos++];
                if (c is '*' or '-' or '_')
                {
                    // All of the non-whitespace characters on the line must match.
                    if (hrCharCount > 0 && c != hrChar)
                    {
                        return null;
                    }

                    hrChar = c;
                    hrCharCount++;
                }
                else if (c == '\n')
                {
                    break;
                }
                else if (!ParseHelpers.IsMarkdownWhiteSpace(c))
                {
                    return null;
                }
            }

            // Hopefully there were at least 3 stars/dashes/underscores.
            return hrCharCount >= 3 ? new HorizontalRuleBlock() : null;
        }

        /// <summary>
        ///     Converts the object into it's textual representation.
        /// </summary>
        /// <returns> The textual representation of this object. </returns>
        public override string ToString()
        {
            return "---";
        }
    }
}