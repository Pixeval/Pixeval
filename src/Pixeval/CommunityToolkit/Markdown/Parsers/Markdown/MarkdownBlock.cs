#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/MarkdownBlock.cs
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
    ///     A Block Element is an element that is a container for other structures.
    /// </summary>
    public abstract class MarkdownBlock : MarkdownElement
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MarkdownBlock" /> class.
        /// </summary>
        internal MarkdownBlock(MarkdownBlockType type)
        {
            Type = type;
        }

        /// <summary>
        ///     Gets or sets tells us what type this element is.
        /// </summary>
        public MarkdownBlockType Type { get; }

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> <c>true</c> if the specified object is equal to the current object; otherwise, <c>false.</c> </returns>
        public override bool Equals(object? obj)
        {
            // ReSharper disable once BaseObjectEqualsIsObjectEquals
            if (!base.Equals(obj) || obj is not MarkdownBlock block)
            {
                return false;
            }

            return Type == block.Type;
        }

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        public override int GetHashCode()
        {
            // ReSharper disable BaseObjectGetHashCodeCallInGetHashCode
            return base.GetHashCode() ^ Type.GetHashCode();
        }
    }
}