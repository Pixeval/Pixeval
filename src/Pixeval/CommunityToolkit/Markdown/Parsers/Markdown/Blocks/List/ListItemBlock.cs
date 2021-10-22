#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/ListItemBlock.cs
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

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Blocks.List
{
    /// <summary>
    ///     This specifies the Content of the List element.
    /// </summary>
    public class ListItemBlock
    {
        internal ListItemBlock()
        {
        }

        /// <summary>
        ///     Gets or sets the contents of the list item.
        /// </summary>
        public IList<MarkdownBlock?>? Blocks { get; set; }
    }
}