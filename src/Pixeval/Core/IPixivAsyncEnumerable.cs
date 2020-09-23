#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;

namespace Pixeval.Core
{
    /// <summary>
    ///     A base class provides several functions to browse the Pixiv content
    /// </summary>
    /// <typeparam name="T">the correspond data type</typeparam>
    public interface IPixivAsyncEnumerable<T> : IAsyncEnumerable<T>, ICancellable
    {
        /// <summary>
        ///     Indicates how many pages have been requested
        /// </summary>
        int RequestedPages { get; }

        /// <summary>
        ///     Basically, this method SHOULD increase the value of <see cref="RequestedPages" />
        /// </summary>
        void ReportRequestedPages();

        /// <summary>
        ///     Tell the <see cref="IPixivAsyncEnumerable{T}" /> how to insert <see cref="item" /> to a <see cref="IList{T}" />
        /// </summary>
        /// <param name="item"></param>
        /// <param name="collection"></param>
        void InsertionPolicy(T item, IList<T> collection);

        /// <summary>
        ///     Check if the <see cref="item" /> has the rationality to be inserted to the <see cref="IList{T}" />
        /// </summary>
        /// <param name="item"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        bool VerifyRationality(T item, IList<T> collection);
    }
}