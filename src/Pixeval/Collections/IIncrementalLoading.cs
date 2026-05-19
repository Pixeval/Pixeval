// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.Collections;

/// <summary>Specifies a calling contract for collection views that support incremental loading.</summary>
public interface IIncrementalLoading
{
    /// <summary>Initializes incremental loading from the view.</summary>
    /// <param name="count">The number of items to load.</param>
    /// <param name="token"></param>
    /// <returns>The count of the load operation.</returns>
    Task<int> LoadMoreItemsAsync(int count, CancellationToken token = default);

    /// <summary>Gets a sentinel value that supports incremental loading implementations.</summary>
    /// <returns>true if additional unloaded items remain in the view; otherwise, false.</returns>
    bool HasMoreItems { get; }
}
