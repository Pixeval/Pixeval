#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/ColumnAlignment.cs
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
    ///     The alignment of content in a table column.
    /// </summary>
    public enum ColumnAlignment
    {
        /// <summary>
        ///     The alignment was not specified.
        /// </summary>
        Unspecified,

        /// <summary>
        ///     Content should be left aligned.
        /// </summary>
        Left,

        /// <summary>
        ///     Content should be right aligned.
        /// </summary>
        Right,

        /// <summary>
        ///     Content should be centered.
        /// </summary>
        Center
    }
}