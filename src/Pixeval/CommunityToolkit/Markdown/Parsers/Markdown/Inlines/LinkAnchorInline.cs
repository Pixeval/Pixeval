#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/LinkAnchorInline.cs
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
using System.Xml.Linq;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Enums;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Helpers;

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Inlines
{
    /// <summary>
    ///     Represents a span that contains a reference for links to point to.
    /// </summary>
    public class LinkAnchorInline : MarkdownInline
    {
        internal LinkAnchorInline()
            : base(MarkdownInlineType.LinkReference)
        {
        }

        /// <summary>
        ///     Gets or sets the Name of this Link Reference.
        /// </summary>
        public string? Link { get; set; }

        /// <summary>
        ///     Gets or sets the raw Link Reference.
        /// </summary>
        public string? Raw { get; set; }

        /// <summary>
        ///     Returns the chars that if found means we might have a match.
        /// </summary>
        internal static void AddTripChars(List<InlineTripCharHelper> tripCharHelpers)
        {
            tripCharHelpers.Add(new InlineTripCharHelper { FirstChar = '<', Method = InlineParseMethod.LinkReference });
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

            // Check the start sequence.
            var startSequence = markdown.Substring(start, 2);
            if (startSequence != "<a")
            {
                return null;
            }

            // Find the end of the span.  The end sequence ('-->')
            var innerStart = start + 2;
            var innerEnd = Common.IndexOf(markdown, "</a>", innerStart, maxEnd);
            var trueEnd = innerEnd + 4;
            if (innerEnd == -1)
            {
                innerEnd = Common.IndexOf(markdown, "/>", innerStart, maxEnd);
                trueEnd = innerEnd + 2;
                if (innerEnd == -1)
                {
                    return null;
                }
            }

            // This link Reference wasn't closed properly if the next link starts before a close.
            var nextLink = Common.IndexOf(markdown, "<a", innerStart, maxEnd);
            if (nextLink > -1 && nextLink < innerEnd)
            {
                return null;
            }

            var length = trueEnd - start;
            var contents = markdown.Substring(start, length);

            string? link = null;

            try
            {
                var xml = XElement.Parse(contents);
                var attr = xml.Attribute("name");
                if (attr != null)
                {
                    link = attr.Value;
                }
            }
            catch
            {
                // Attempting to fetch link failed, ignore and continue.
            }

            // Remove whitespace if it exists.
            if (trueEnd + 1 <= maxEnd && markdown[trueEnd] == ' ')
            {
                trueEnd += 1;
            }

            // We found something!
            var result = new LinkAnchorInline
            {
                Raw = contents,
                Link = link
            };
            return new InlineParseResult(result, start, trueEnd);
        }

        /// <summary>
        ///     Converts the object into it's textual representation.
        /// </summary>
        /// <returns> The textual representation of this object. </returns>
        public override string? ToString()
        {
            return Raw;
        }
    }
}