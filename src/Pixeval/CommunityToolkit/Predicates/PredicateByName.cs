#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/PredicateByName.cs
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
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;

namespace Pixeval.CommunityToolkit.Predicates
{
    /// <summary>
    ///     An <see cref="IPredicate{T}" /> type matching <see cref="FrameworkElement" /> instances by name.
    /// </summary>
    internal readonly struct PredicateByName : IPredicate<FrameworkElement>
    {
        /// <summary>
        ///     The name of the element to look for.
        /// </summary>
        private readonly string name;

        /// <summary>
        ///     The comparison type to use to match <see name="name" />.
        /// </summary>
        private readonly StringComparison comparisonType;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PredicateByName" /> struct.
        /// </summary>
        /// <param name="name">The name of the element to look for.</param>
        /// <param name="comparisonType">The comparison type to use to match <paramref name="name" />.</param>
        public PredicateByName(string name, StringComparison comparisonType)
        {
            this.name = name;
            this.comparisonType = comparisonType;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(FrameworkElement element)
        {
            return element.Name.Equals(name, comparisonType);
        }
    }
}