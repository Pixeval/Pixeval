#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/InlineParseMethod.cs
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

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Enums
{
    internal enum InlineParseMethod
    {
        /// <summary>
        ///     A Comment text
        /// </summary>
        Comment,

        /// <summary>
        ///     A Link Reference
        /// </summary>
        LinkReference,

        /// <summary>
        ///     A bold element
        /// </summary>
        Bold,

        /// <summary>
        ///     An bold and italic block
        /// </summary>
        BoldItalic,

        /// <summary>
        ///     A code element
        /// </summary>
        Code,

        /// <summary>
        ///     An italic block
        /// </summary>
        Italic,

        /// <summary>
        ///     A link block
        /// </summary>
        MarkdownLink,

        /// <summary>
        ///     An angle bracket link.
        /// </summary>
        AngleBracketLink,

        /// <summary>
        ///     A url block
        /// </summary>
        Url,

        /// <summary>
        ///     A reddit style link
        /// </summary>
        RedditLink,

        /// <summary>
        ///     An in line text link
        /// </summary>
        PartialLink,

        /// <summary>
        ///     An email element
        /// </summary>
        Email,

        /// <summary>
        ///     strike through element
        /// </summary>
        StrikeThrough,

        /// <summary>
        ///     Super script element.
        /// </summary>
        Superscript,

        /// <summary>
        ///     Sub script element.
        /// </summary>
        Subscript,

        /// <summary>
        ///     Image element.
        /// </summary>
        Image,

        /// <summary>
        ///     Emoji element.
        /// </summary>
        Emoji
    }
}