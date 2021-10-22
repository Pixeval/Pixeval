#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IIncrementalSource.cs
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
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.CommunityToolkit.IncrementalLoadingCollection
{
    /// <summary>
    ///     This interface represents a data source whose items can be loaded incrementally.
    /// </summary>
    /// <typeparam name="TSource">Type of collection element.</typeparam>
    public interface IIncrementalSource<TSource>
    {
        /// <summary>
        ///     This method is invoked every time the view need to show more items. Retrieves items based on
        ///     <paramref name="pageIndex" /> and <paramref name="pageSize" /> arguments.
        /// </summary>
        /// <param name="pageIndex">
        ///     The zero-based index of the page that corresponds to the items to retrieve.
        /// </param>
        /// <param name="pageSize">
        ///     The number of <typeparamref name="TSource" /> items to retrieve for the specified <paramref name="pageIndex" />.
        /// </param>
        /// <param name="cancellationToken">
        ///     Used to propagate notification that operation should be canceled.
        /// </param>
        /// <returns>
        ///     Returns a collection of <typeparamref name="TSource" />.
        /// </returns>
        Task<IEnumerable<TSource>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    }
}