#region Copyright (c) Pixeval/Pixeval.Utilities

// GPL v3 License
// 
// Pixeval/Pixeval.Utilities
// Copyright (c) 2021 Pixeval.Utilities/AsyncEnumerableAdapters.cs
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
using JetBrains.Annotations;

namespace Pixeval.Utilities;

[PublicAPI]
public class AdaptedAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly CancellationToken _cancellationToken;
    private readonly IEnumerator<T> _outerEnumerator;

    public AdaptedAsyncEnumerator(IEnumerator<T> outerEnumerator, CancellationToken cancellationToken = new())
    {
        _outerEnumerator = outerEnumerator;
        _cancellationToken = cancellationToken;
    }

    public ValueTask DisposeAsync()
    {
        _outerEnumerator.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(!_cancellationToken.IsCancellationRequested && _outerEnumerator.MoveNext());
    }

    public T Current => _outerEnumerator.Current;
}

[PublicAPI]
public class AdaptedAsyncEnumerable<T> : IAsyncEnumerable<T>
{
    private readonly IEnumerable<T> _sync;

    public AdaptedAsyncEnumerable(IEnumerable<T> sync)
    {
        _sync = sync;
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return new AdaptedAsyncEnumerator<T>(_sync.GetEnumerator());
    }
}