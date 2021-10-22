#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/ParagraphBlock.cs
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

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Blocks
{
    /// <summary>
    ///     Represents a block of text that is displayed as a single paragraph.
    /// </summary>
    public class ParagraphBlock : MarkdownBlock
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ParagraphBlock" /> class.
        /// </summary>
        public ParagraphBlock()
            : base(MarkdownBlockType.Paragraph)
        {
        }

        /// <summary>
        ///     Gets or sets the contents of the block.
        /// </summary>
        public IList<MarkdownInline>? Inlines { get; set; }

        /// <summary>
        ///     Parses paragraph text.
        /// </summary>
        /// <param name="markdown"> The markdown text. </param>
        /// <returns> A parsed paragraph. </returns>
        internal static ParagraphBlock Parse(string markdown)
        {
            var result = new ParagraphBlock
            {
                Inlines = Common.ParseInlineChildren(markdown, 0, markdown.Length)
            };
            return result;
        }

        /// <summary>
        ///     Converts the object into it's textual representation.
        /// </summary>
        /// <returns> The textual representation of this object. </returns>
        public override string? ToString()
        {
            return Inlines == null ? base.ToString() : string.Join(string.Empty, Inlines);
        }
    }
}