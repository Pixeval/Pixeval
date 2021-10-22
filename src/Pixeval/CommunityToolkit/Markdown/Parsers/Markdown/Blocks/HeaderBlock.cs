#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/HeaderBlock.cs
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

using System;
using System.Collections.Generic;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Enums;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Helpers;

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Blocks
{
    /// <summary>
    ///     Represents a heading.
    ///     <seealso href="https://spec.commonmark.org/0.29/#atx-headings">Single-Line Header CommonMark Spec</seealso>
    ///     <seealso href="https://spec.commonmark.org/0.29/#setext-headings">Two-Line Header CommonMark Spec</seealso>
    /// </summary>
    public class HeaderBlock : MarkdownBlock
    {
        private int _headerLevel;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HeaderBlock" /> class.
        /// </summary>
        public HeaderBlock() : base(MarkdownBlockType.Header)
        {
        }

        /// <summary>
        ///     Gets or sets the header level (1-6).  1 is the most important header, 6 is the least important.
        /// </summary>
        public int HeaderLevel
        {
            get => _headerLevel;

            set
            {
                if (value is < 1 or > 6)
                {
                    // ReSharper disable once NotResolvedInText
                    throw new ArgumentOutOfRangeException("HeaderLevel", @"The header level must be between 1 and 6 (inclusive).");
                }

                _headerLevel = value;
            }
        }

        /// <summary>
        ///     Gets or sets the contents of the block.
        /// </summary>
        public IList<MarkdownInline>? Inlines { get; set; }

        /// <summary>
        ///     Parses a header that starts with a hash.
        /// </summary>
        /// <param name="markdown"> The markdown text. </param>
        /// <param name="start"> The location of the first hash character. </param>
        /// <param name="end"> The location of the end of the line. </param>
        /// <returns> A parsed header block, or <c>null</c> if this is not a header. </returns>
        internal static HeaderBlock? ParseHashPrefixedHeader(string markdown, int start, int end)
        {
            // This type of header starts with one or more '#' characters, followed by the header
            // text, optionally followed by any number of hash characters.
            var result = new HeaderBlock();

            // Figure out how many consecutive hash characters there are.
            var pos = start;
            while (pos < end && markdown[pos] == '#' && pos - start < 6)
            {
                pos++;
            }

            result.HeaderLevel = pos - start;
            if (result.HeaderLevel == 0)
            {
                return null;
            }

            // Ignore any hashes at the end of the line.
            while (pos < end && markdown[end - 1] == '#')
            {
                end--;
            }

            // Parse the inline content.
            result.Inlines = Common.ParseInlineChildren(markdown, pos, end);
            return result;
        }

        /// <summary>
        ///     Parses a two-line header.
        /// </summary>
        /// <param name="markdown"> The markdown text. </param>
        /// <param name="firstLineStart"> The location of the start of the first line. </param>
        /// <param name="firstLineEnd"> The location of the end of the first line. </param>
        /// <param name="secondLineStart"> The location of the start of the second line. </param>
        /// <param name="secondLineEnd"> The location of the end of the second line. </param>
        /// <returns> A parsed header block, or <c>null</c> if this is not a header. </returns>
        internal static HeaderBlock? ParseUnderlineStyleHeader(string markdown, int firstLineStart, int firstLineEnd, int secondLineStart, int secondLineEnd)
        {
            // This type of header starts with some text on the first line, followed by one or more
            // underline characters ('=' or '-') on the second line.
            // The second line can have whitespace after the underline characters, but not before
            // or between each character.

            // Check the second line is valid.
            if (secondLineEnd <= secondLineStart)
            {
                return null;
            }

            // Figure out what the underline character is ('=' or '-').
            var underlineChar = markdown[secondLineStart];
            if (underlineChar != '=' && underlineChar != '-')
            {
                return null;
            }

            // Read past consecutive underline characters.
            var pos = secondLineStart + 1;
            for (; pos < secondLineEnd; pos++)
            {
                var c = markdown[pos];
                if (c != underlineChar)
                {
                    break;
                }

                pos++;
            }

            // The rest of the line must be whitespace.
            for (; pos < secondLineEnd; pos++)
            {
                var c = markdown[pos];
                if (c != ' ' && c != '\t')
                {
                    return null;
                }

                pos++;
            }

            var result = new HeaderBlock
            {
                HeaderLevel = underlineChar == '=' ? 1 : 2,
                // Parse the inline content.
                Inlines = Common.ParseInlineChildren(markdown, firstLineStart, firstLineEnd)
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