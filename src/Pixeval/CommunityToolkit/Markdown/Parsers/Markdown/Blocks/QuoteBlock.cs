#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/QuoteBlock.cs
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

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Blocks
{
    /// <summary>
    ///     Represents a block that is displayed using a quote style.  Quotes are used to indicate
    ///     that the text originated elsewhere (e.g. a previous comment).
    /// </summary>
    public class QuoteBlock : MarkdownBlock
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="QuoteBlock" /> class.
        /// </summary>
        public QuoteBlock()
            : base(MarkdownBlockType.Quote)
        {
        }

        /// <summary>
        ///     Gets or sets the contents of the block.
        /// </summary>
        public IList<MarkdownBlock>? Blocks { get; set; }

        /// <summary>
        ///     Parses a quote block.
        /// </summary>
        /// <param name="markdown"> The markdown text. </param>
        /// <param name="startOfLine"> The location of the start of the line. </param>
        /// <param name="maxEnd"> The location to stop parsing. </param>
        /// <param name="quoteDepth"> The current nesting level of quotes. </param>
        /// <param name="actualEnd"> Set to the end of the block when the return value is non-null. </param>
        /// <returns> A parsed quote block. </returns>
        internal static QuoteBlock Parse(string markdown, int startOfLine, int maxEnd, int quoteDepth, out int actualEnd)
        {
            var result = new QuoteBlock
            {
                // Recursively call into the markdown block parser.
                Blocks = MarkdownDocument.Parse(markdown, startOfLine, maxEnd, quoteDepth + 1, out actualEnd)
            };

            return result;
        }
    }
}