// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Collections;
using Pixeval.Controls;

namespace Pixeval.Pages;

public class CommentsIncrementalSource(IAsyncEnumerable<CommentItemViewModel?> source) : IIncrementalSource<CommentItemViewModel>
{
    public async Task<IEnumerable<CommentItemViewModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        return (await source.Skip(pageIndex * pageSize).Take(pageSize).ToArrayAsync(cancellationToken))!;
    }
}
