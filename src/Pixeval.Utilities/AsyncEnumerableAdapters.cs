// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

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
