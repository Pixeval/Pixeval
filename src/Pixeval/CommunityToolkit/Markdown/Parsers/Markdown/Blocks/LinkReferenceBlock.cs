#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/LinkReferenceBlock.cs
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
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Inlines;

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Blocks
{
    /// <summary>
    ///     Represents the target of a reference ([ref][]).
    /// </summary>
    public class LinkReferenceBlock : MarkdownBlock
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LinkReferenceBlock" /> class.
        /// </summary>
        public LinkReferenceBlock()
            : base(MarkdownBlockType.LinkReference)
        {
        }

        /// <summary>
        ///     Gets or sets the reference ID.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        ///     Gets or sets the link URL.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        ///     Gets or sets a tooltip to display on hover.
        /// </summary>
        public string? Tooltip { get; set; }

        /// <summary>
        ///     Attempts to parse a reference e.g. "[example]: http://www.reddit.com 'title'".
        /// </summary>
        /// <param name="markdown"> The markdown text. </param>
        /// <param name="start"> The location to start parsing. </param>
        /// <param name="end"> The location to stop parsing. </param>
        /// <returns> A parsed markdown link, or <c>null</c> if this is not a markdown link. </returns>
        internal static LinkReferenceBlock? Parse(string markdown, int start, int end)
        {
            // Expect a '[' character.
            if (start >= end || markdown[start] != '[')
            {
                return null;
            }

            // Find the ']' character
            var pos = start + 1;
            while (pos < end)
            {
                if (markdown[pos] == ']')
                {
                    break;
                }

                pos++;
            }

            if (pos == end)
            {
                return null;
            }

            // Extract the ID.
            var id = markdown.Substring(start + 1, pos - (start + 1));

            // Expect the ':' character.
            pos++;
            if (pos == end || markdown[pos] != ':')
            {
                return null;
            }

            // Skip whitespace
            pos++;
            while (pos < end && ParseHelpers.IsMarkdownWhiteSpace(markdown[pos]))
            {
                pos++;
            }

            if (pos == end)
            {
                return null;
            }

            // Extract the URL.
            var urlStart = pos;
            while (pos < end && !ParseHelpers.IsMarkdownWhiteSpace(markdown[pos]))
            {
                pos++;
            }

            var url = TextRunInline.ResolveEscapeSequences(markdown, urlStart, pos);

            // Ignore leading '<' and trailing '>'.
            url = url.TrimStart('<').TrimEnd('>');

            // Skip whitespace.
            pos++;
            while (pos < end && ParseHelpers.IsMarkdownWhiteSpace(markdown[pos]))
            {
                pos++;
            }

            string? tooltip = null;
            if (pos < end)
            {
                // Extract the tooltip.
                char tooltipEndChar;
                switch (markdown[pos])
                {
                    case '(':
                        tooltipEndChar = ')';
                        break;

                    case '"':
                    case '\'':
                        tooltipEndChar = markdown[pos];
                        break;

                    default:
                        return null;
                }

                pos++;
                var tooltipStart = pos;
                while (pos < end && markdown[pos] != tooltipEndChar)
                {
                    pos++;
                }

                if (pos == end)
                {
                    return null; // No end character.
                }

                tooltip = markdown.Substring(tooltipStart, pos - tooltipStart);

                // Check there isn't any trailing text.
                pos++;
                while (pos < end && ParseHelpers.IsMarkdownWhiteSpace(markdown[pos]))
                {
                    pos++;
                }

                if (pos < end)
                {
                    return null;
                }
            }

            // We found something!
            var result = new LinkReferenceBlock
            {
                Id = id,
                Url = url,
                Tooltip = tooltip
            };
            return result;
        }

        /// <summary>
        ///     Converts the object into it's textual representation.
        /// </summary>
        /// <returns> The textual representation of this object. </returns>
        public override string ToString()
        {
            return $"[{Id}]: {Url} {Tooltip}";
        }
    }
}