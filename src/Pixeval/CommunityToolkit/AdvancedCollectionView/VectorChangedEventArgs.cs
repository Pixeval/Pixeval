#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/VectorChangedEventArgs.cs
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

using Windows.Foundation.Collections;

namespace Pixeval.CommunityToolkit.AdvancedCollectionView
{
    /// <summary>
    ///     Vector changed EventArgs
    /// </summary>
    internal class VectorChangedEventArgs : IVectorChangedEventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VectorChangedEventArgs" /> class.
        /// </summary>
        /// <param name="cc">collection change type</param>
        /// <param name="index">index of item changed</param>
        public VectorChangedEventArgs(CollectionChange cc, int index = -1)
        {
            CollectionChange = cc;
            Index = (uint) index;
        }

        /// <summary>
        ///     Gets the type of change that occurred in the vector.
        /// </summary>
        /// <returns>
        ///     The type of change in the vector.
        /// </returns>
        public CollectionChange CollectionChange { get; }

        /// <summary>
        ///     Gets the position where the change occurred in the vector.
        /// </summary>
        /// <returns>
        ///     The zero-based position where the change occurred in the vector, if applicable.
        /// </returns>
        public uint Index { get; }
    }
}