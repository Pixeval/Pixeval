#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/SortDescription.cs
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

using System;
using System.Collections;

namespace Pixeval.CommunityToolkit.AdvancedCollectionView
{
    /// <summary>
    ///     Sort description
    /// </summary>
    public class SortDescription
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SortDescription" /> class that describes
        ///     a sort on the object itself
        /// </summary>
        /// <param name="direction">Direction of sort</param>
        /// <param name="comparer">Comparer to use. If null, will use default comparer</param>
        public SortDescription(SortDirection direction, IComparer? comparer = null)
            : this(null, direction, comparer)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SortDescription" /> class.
        /// </summary>
        /// <param name="propertyName">Name of property to sort on</param>
        /// <param name="direction">Direction of sort</param>
        /// <param name="comparer">Comparer to use. If null, will use default comparer</param>
        public SortDescription(string? propertyName, SortDirection direction, IComparer? comparer = null)
        {
            PropertyName = propertyName;
            Direction = direction;
            Comparer = comparer ?? ObjectComparer.Instance;
        }

        /// <summary>
        ///     Gets the name of property to sort on
        /// </summary>
        public string? PropertyName { get; }

        /// <summary>
        ///     Gets the direction of sort
        /// </summary>
        public SortDirection Direction { get; }

        /// <summary>
        ///     Gets the comparer
        /// </summary>
        public IComparer Comparer { get; }

        private class ObjectComparer : IComparer
        {
            public static readonly IComparer Instance = new ObjectComparer();

            private ObjectComparer()
            {
            }

            public int Compare(object? x, object? y)
            {
                var cx = x as IComparable;
                var cy = y as IComparable;

                // ReSharper disable once PossibleUnintendedReferenceComparison
                return cx == cy ? 0 : cx == null ? -1 : cy == null ? +1 : cx.CompareTo(cy);
            }
        }
    }
}