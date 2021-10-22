#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/MarkdownInlineType.cs
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
    /// <summary>
    ///     Determines the type of Inline the Inline Element is.
    /// </summary>
    public enum MarkdownInlineType
    {
        /// <summary>
        ///     A comment
        /// </summary>
        Comment,

        /// <summary>
        ///     A text run
        /// </summary>
        TextRun,

        /// <summary>
        ///     A bold run
        /// </summary>
        Bold,

        /// <summary>
        ///     An italic run
        /// </summary>
        Italic,

        /// <summary>
        ///     A link in markdown syntax
        /// </summary>
        MarkdownLink,

        /// <summary>
        ///     A raw hyper link
        /// </summary>
        RawHyperlink,

        /// <summary>
        ///     A raw subreddit link
        /// </summary>
        RawSubreddit,

        /// <summary>
        ///     A strike through run
        /// </summary>
        Strikethrough,

        /// <summary>
        ///     A superscript run
        /// </summary>
        Superscript,

        /// <summary>
        ///     A subscript run
        /// </summary>
        Subscript,

        /// <summary>
        ///     A code run
        /// </summary>
        Code,

        /// <summary>
        ///     An image
        /// </summary>
        Image,

        /// <summary>
        ///     Emoji
        /// </summary>
        Emoji,

        /// <summary>
        ///     Link Reference
        /// </summary>
        LinkReference
    }
}