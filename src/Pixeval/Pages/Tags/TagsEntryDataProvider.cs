using System.Collections.Generic;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Pixeval.Collections;
using Pixeval.Controls.Illustrate;
using Pixeval.CoreApi.Engine;
using Pixeval.Misc;
using Pixeval.Pages.Download;
using WinUI3Utilities;

namespace Pixeval.Pages.Tags;

public class TagsEntryDataProvider : ObservableObject, IDataProvider<FileInfo, TagsEntryViewModel>
{
    public AdvancedObservableCollection<TagsEntryViewModel> View { get; } = [];

    public IncrementalLoadingCollection<FetchEngineIncrementalSource<FileInfo, TagsEntryViewModel>, TagsEntryViewModel> Source
    {
        get => (View.Source as IncrementalLoadingCollection<FetchEngineIncrementalSource<FileInfo, TagsEntryViewModel>, TagsEntryViewModel>)!;
        protected set => View.Source = value;
    }

    public IFetchEngine<FileInfo?>? FetchEngine { get; protected set; }

    public void Dispose()
    {
        if (Source is { } source)
            foreach (var entry in source)
                entry.Dispose();

        View.Clear();
    }

    void IDataProvider<FileInfo, TagsEntryViewModel>.ResetEngine(IFetchEngine<FileInfo?>? fetchEngine, int limit)
    {
        ThrowHelper.NotSupported($"{nameof(DownloadListEntryDataProvider)} 不使用 {nameof(FetchEngine)}");
    }

    public void ResetEngine(IEnumerable<FileInfo> source)
    {
        Source = new IncrementalLoadingCollection<FetchEngineIncrementalSource<FileInfo, TagsEntryViewModel>, TagsEntryViewModel>(new TagsEntryIncrementalSource(source));
    }
}
