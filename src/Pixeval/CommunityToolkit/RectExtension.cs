#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/RectExtension.cs
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

using System.Runtime.CompilerServices;
using Windows.Foundation;
using JetBrains.Annotations;

namespace Pixeval.CommunityToolkit
{
    /// <summary>
    ///     Extensions for the <see cref="Rect" /> type.
    /// </summary>
    public static class RectExtensions
    {
        /// <summary>
        ///     Determines if a rectangle intersects with another rectangle.
        /// </summary>
        /// <param name="rect1">The first rectangle to test.</param>
        /// <param name="rect2">The second rectangle to test.</param>
        /// <returns>This method returns <see langword="true" /> if there is any intersection, otherwise <see langword="false" />.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IntersectsWith(this Rect rect1, Rect rect2)
        {
            if (rect1.IsEmpty || rect2.IsEmpty)
            {
                return false;
            }

            return rect1.Left <= rect2.Right &&
                   rect1.Right >= rect2.Left &&
                   rect1.Top <= rect2.Bottom &&
                   rect1.Bottom >= rect2.Top;
        }

        /// <summary>
        ///     Creates a new <see cref="Size" /> of the specified <see cref="Rect" />'s width and height.
        /// </summary>
        /// <param name="rect">Rectangle to size.</param>
        /// <returns>Size of rectangle.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Size ToSize(this Rect rect)
        {
            return new Size(rect.Width, rect.Height);
        }
    }
}