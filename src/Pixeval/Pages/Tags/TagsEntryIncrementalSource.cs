using Pixeval.Misc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using WinUI3Utilities;

namespace Pixeval.Pages.Tags;

public class TagsEntryIncrementalSource(IEnumerable<FileInfo> source)
    : FetchEngineIncrementalSource<FileInfo, TagsEntryViewModel>(null!)
{
    protected override long Identifier(FileInfo entity) => ThrowHelper.NotSupported<long>();

    protected override TagsEntryViewModel Select(FileInfo entity) => ThrowHelper.NotSupported<TagsEntryViewModel>();

    public override async Task<IEnumerable<TagsEntryViewModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = new CancellationToken())
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
