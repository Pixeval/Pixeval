// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;

namespace Mako.Engine;

public class ComputedFetchEngine<T>(IAsyncEnumerable<T> result, MakoClient makoClient, EngineHandle engineHandle)
    : IFetchEngine<T>
{
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
    {
        return result.GetAsyncEnumerator(cancellationToken);
    }

    public MakoClient MakoClient { get; } = makoClient;

    public EngineHandle EngineHandle { get; } = engineHandle;

    /// <summary>
    /// The <see cref="RequestedPages"/> in <see cref="ComputedFetchEngine{T}"/> should always returns -1
    /// </summary>
    public int RequestedPages { get; set; } = -1;
}
