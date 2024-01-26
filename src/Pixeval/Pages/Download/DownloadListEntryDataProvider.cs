using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Pixeval.Collections;
using Pixeval.Controls.Illustrate;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Download.Models;
using Pixeval.Misc;
using WinUI3Utilities;

namespace Pixeval.Pages.Download;

public class DownloadListEntryDataProvider : ObservableObject, IDataProvider<Illustration, DownloadListEntryViewModel>
{
    public AdvancedObservableCollection<DownloadListEntryViewModel> View { get; } = [];

    public IncrementalLoadingCollection<FetchEngineIncrementalSource<Illustration, DownloadListEntryViewModel>, DownloadListEntryViewModel> Source
    {
        get => (View.Source as IncrementalLoadingCollection<FetchEngineIncrementalSource<Illustration, DownloadListEntryViewModel>, DownloadListEntryViewModel>)!;
        protected set => View.Source = value;
    }

    public IFetchEngine<Illustration?>? FetchEngine { get; protected set; }

    public void Dispose()
    {
        if (Source is { } source)
            foreach (var illustrationViewModel in source)
            {
                illustrationViewModel.UnloadThumbnail(this);
                illustrationViewModel.Dispose();
            }

        View.Clear();
    }

    void IDataProvider<Illustration, DownloadListEntryViewModel>.ResetEngine(IFetchEngine<Illustration?>? fetchEngine, int limit)
    {
        ThrowHelper.NotSupported($"{nameof(DownloadListEntryDataProvider)} 不使用 {nameof(FetchEngine)}");
    }

    public void ResetEngine(IEnumerable<IllustrationDownloadTask> source)
    {
        Source = new IncrementalLoadingCollection<FetchEngineIncrementalSource<Illustration, DownloadListEntryViewModel>, DownloadListEntryViewModel>(new DownloadListEntryIncrementalSource(source));
    }
}
