#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IPredicate.cs
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

namespace Pixeval.CommunityToolkit.Predicates
{
    /// <summary>
    ///     An interface representing a predicate for items of a given type.
    /// </summary>
    /// <typeparam name="T">The type of items to match.</typeparam>
    internal interface IPredicate<in T>
        where T : class
    {
        /// <summary>
        ///     Performs a match with the current predicate over a target <typeparamref name="T" /> instance.
        /// </summary>
        /// <param name="element">The input element to match.</param>
        /// <returns>Whether the match evaluation was successful.</returns>
        bool Match(T element);
    }
}