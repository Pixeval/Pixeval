// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using CommunityToolkit.WinUI.Collections;

namespace Pixeval.Pages.Tags;

public class TagsEntryIncrementalSource(IEnumerable<FileInfo> source) : IIncrementalSource<TagsEntryViewModel>
{
    public async Task<IEnumerable<TagsEntryViewModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var list = new List<TagsEntryViewModel>();
        foreach (var fileInfo in source.Skip(pageIndex * pageSize))
        {
            if (await TagsEntryViewModel.IdentifyAsync(fileInfo.FullName) is { } entry)
            {
                list.Add(entry);
                if (list.Count >= pageSize)
                    return list;
            }
        }
        return list;
    }
}
