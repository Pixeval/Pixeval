#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/MarkdownBlockType.cs
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
    ///     Determines the type of Block the Block element is.
    /// </summary>
    public enum MarkdownBlockType
    {
        /// <summary>
        ///     The root element
        /// </summary>
        Root,

        /// <summary>
        ///     A paragraph element.
        /// </summary>
        Paragraph,

        /// <summary>
        ///     A quote block
        /// </summary>
        Quote,

        /// <summary>
        ///     A code block
        /// </summary>
        Code,

        /// <summary>
        ///     A header block
        /// </summary>
        Header,

        /// <summary>
        ///     A list block
        /// </summary>
        List,

        /// <summary>
        ///     A list item block
        /// </summary>
        ListItemBuilder,

        /// <summary>
        ///     a horizontal rule block
        /// </summary>
        HorizontalRule,

        /// <summary>
        ///     A table block
        /// </summary>
        Table,

        /// <summary>
        ///     A link block
        /// </summary>
        LinkReference,

        /// <summary>
        ///     A Yaml header block
        /// </summary>
        YamlHeader
    }
}