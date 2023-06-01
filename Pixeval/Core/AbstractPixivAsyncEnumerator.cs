﻿#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

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
using System.Threading.Tasks;

namespace Pixeval.Core
{
    /// <summary>
    ///     Provide a set of functions that support iterate an <see cref="IPixivAsyncEnumerable{T}" />
    /// </summary>
    /// <typeparam name="T">the correspond data type</typeparam>
    public abstract class AbstractPixivAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        protected IPixivAsyncEnumerable<T> Enumerable;

        protected AbstractPixivAsyncEnumerator(IPixivAsyncEnumerable<T> enumerable)
        {
            Enumerable = enumerable;
        }

        public ValueTask DisposeAsync()
        {
            return default;
        }

        public abstract ValueTask<bool> MoveNextAsync();

        public abstract T Current { get; }

        protected abstract void UpdateEnumerator();
    }
}