#region Copyright (c) Pixeval/Pixeval.Utilities
// GPL v3 License
// 
// Pixeval/Pixeval.Utilities
// Copyright (c) 2023 Pixeval.Utilities/AsyncEnumerableAdapters.cs
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.Utilities;

public class AdaptedAsyncEnumerator<T>(IEnumerator<T> outerEnumerator, CancellationToken cancellationToken = new CancellationToken())
    : IAsyncEnumerator<T>
{
    public ValueTask DisposeAsync()
    {
        outerEnumerator.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(!cancellationToken.IsCancellationRequested && outerEnumerator.MoveNext());
    }

    public T Current => outerEnumerator.Current;
}

public class AdaptedAsyncEnumerable<T>(IEnumerable<T> sync) : IAsyncEnumerable<T>
{
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
    {
        return new AdaptedAsyncEnumerator<T>(sync.GetEnumerator());
    }
}
