using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Pixeval.Collections;
using Pixeval.Controls.Illustrate;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Download;
using Pixeval.Misc;

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

    public void DisposeCurrent()
    {
        if (Source is { } source)
            foreach (var downloadListEntryViewModel in source)
                downloadListEntryViewModel.Dispose();

        View.Clear();
    }

    public void ResetEngine(IEnumerable<ObservableDownloadTask> source)
    {
        Source = new(new DownloadListEntryIncrementalSource(source));
    }

    void IDataProvider<Illustration, DownloadListEntryViewModel>.ResetEngine(IFetchEngine<Illustration?>? fetchEngine, int limit)
    {
        throw new NotImplementedException("DownloadListEntryDataProvider 不使用 FetchEngine");
    }
}
