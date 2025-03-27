// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Mako.Global;

namespace Mako.Engine;

/// <summary>
/// A highly abstracted fetch engine that fetches pages and yields results asynchronously
/// <para>
///     Just like a fetch engine, it continuously fetches pages, and each page may contain either multiple
///     result entries or an error response, at each iteration, it fetches a single page and tries to
///     deserialize its content into a list of result entries or stops and reports the iteration is over
/// </para>
/// </summary>
/// <typeparam name="TResult">The type of the results of the <see cref="IFetchEngine{TE}" /></typeparam>
public interface IFetchEngine<out TResult> : IAsyncEnumerable<TResult>, IMakoClientSupport, IEngineHandleSource
{
    /// <summary>
    /// How many pages have been fetches
    /// </summary>
    int RequestedPages { get; set; }
}
