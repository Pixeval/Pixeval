#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/MarkdownInline.cs
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

using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Enums;

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown
{
    /// <summary>
    ///     An internal class that is the base class for all inline elements.
    /// </summary>
    public abstract class MarkdownInline : MarkdownElement
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MarkdownInline" /> class.
        /// </summary>
        internal MarkdownInline(MarkdownInlineType type)
        {
            Type = type;
        }

        /// <summary>
        ///     Gets or sets this element is.
        /// </summary>
        public MarkdownInlineType Type { get; set; }
    }
}