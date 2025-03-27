// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Threading;

namespace Mako.Engine;

/// <summary>
/// A <see cref="IFetchEngine{E}" /> that specialized for Pixiv, it holds an <see cref="MakoClient" />
/// and a <see cref="EngineHandle" /> to manage its lifetime
/// </summary>
/// <typeparam name="TE">
/// <inheritdoc cref="IFetchEngine{TE}" />
/// </typeparam>
public abstract class AbstractPixivFetchEngine<TE>(MakoClient makoClient, EngineHandle? engineHandle) : IFetchEngine<TE>
{
    public abstract IAsyncEnumerator<TE> GetAsyncEnumerator(CancellationToken cancellationToken = new()); // the 'CancellationToken' is no longer useful, we use 'EngineHandle' to track the lifetime

    /// <summary>
    /// The <see cref="MakoClient" /> that owns this <see cref="IFetchEngine{TE}" />, it
    /// shares its context such as <see cref="MakoClient.Configuration" /> with current
    /// <see cref="IFetchEngine{TE}" /> to provides the required fields when the <see cref="IFetchEngine{E}" />
    /// performing its task
    /// </summary>
    public MakoClient MakoClient { get; } = makoClient;

    /// <summary>
    /// How many pages have been fetched
    /// </summary>
    public int RequestedPages { get; set; }

    /// <summary>
    /// The <see cref="EngineHandle" /> used to manage the lifetime of <see cref="IFetchEngine{E}" />
    /// </summary>
    public EngineHandle EngineHandle { get; } = engineHandle ?? new EngineHandle(Guid.NewGuid());
}
