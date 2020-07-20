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
using System.Threading;

namespace Pixeval.Core
{
    /// <summary>
    ///     Abstract implementation of <see cref="IPixivAsyncEnumerable{T}" />, provides the default implementation of cancel a
    ///     running <see cref="IPixivAsyncEnumerable{T}" /> instance
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractPixivAsyncEnumerable<T> : IPixivAsyncEnumerable<T>
    {
        protected bool IsCancelled { get; private set; }

        public abstract int RequestedPages { get; protected set; }

        public abstract IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default);

        public virtual bool VerifyRationality(T item, IList<T> collection)
        {
            return item != null && !collection.Contains(item);
        }

        public virtual void InsertionPolicy(T item, IList<T> collection)
        {
            if (item != null) collection.Add(item);
        }

        public void Cancel()
        {
            IsCancelled = true;
        }

        public bool IsCancellationRequested()
        {
            return IsCancelled;
        }

        public void ReportRequestedPages()
        {
            RequestedPages++;
        }
    }
}
