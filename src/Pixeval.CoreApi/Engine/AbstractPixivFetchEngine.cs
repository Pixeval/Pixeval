#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/AbstractPixivFetchEngine.cs
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

namespace Pixeval.CoreApi.Engine;

/// <summary>
///     A <see cref="IFetchEngine{E}" /> that specialized for Pixiv, it holds an <see cref="MakoClient" />
///     and a <see cref="EngineHandle" /> to manage its lifetime
/// </summary>
/// <typeparam name="E">
///     <inheritdoc cref="IFetchEngine{E}" />
/// </typeparam>
public abstract class AbstractPixivFetchEngine<E>(MakoClient makoClient, EngineHandle? engineHandle) : IFetchEngine<E>
{
    public abstract IAsyncEnumerator<E> GetAsyncEnumerator(CancellationToken cancellationToken = new()); // the 'CancellationToken' is no longer useful, we use 'EngineHandle' to track the lifetime

    /// <summary>
    ///     The <see cref="MakoClient" /> that owns this <see cref="IFetchEngine{E}" />, it
    ///     shares its context such as <see cref="CoreApi.MakoClient.Configuration" /> with current
    ///     <see cref="IFetchEngine{E}" /> to provides the required fields when the <see cref="IFetchEngine{E}" />
    ///     performing its task
    /// </summary>
    public MakoClient MakoClient { get; } = makoClient;

    /// <summary>
    ///     How many pages have been fetched
    /// </summary>
    public int RequestedPages { get; set; }

    /// <summary>
    ///     The <see cref="EngineHandle" /> used to manage the lifetime of <see cref="IFetchEngine{E}" />
    /// </summary>
    public EngineHandle EngineHandle { get; } = engineHandle ?? new EngineHandle(Guid.NewGuid());
}