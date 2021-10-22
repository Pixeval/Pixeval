#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/PredicateByType.cs
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

namespace Pixeval.CommunityToolkit.Predicates
{
    /// <summary>
    ///     An <see cref="IPredicate{T}" /> type matching items of a given type.
    /// </summary>
    internal readonly struct PredicateByType : IPredicate<object>
    {
        /// <summary>
        ///     The type of element to match.
        /// </summary>
        private readonly Type type;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PredicateByType" /> struct.
        /// </summary>
        /// <param name="type">The type of element to match.</param>
        public PredicateByType(Type type)
        {
            this.type = type;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(object element)
        {
            return element.GetType() == type;
        }
    }
}