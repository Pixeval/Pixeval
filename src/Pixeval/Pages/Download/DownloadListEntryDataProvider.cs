using System;
using System.Collections.Generic;
using CommunityToolkit.WinUI.Collections;
using Pixeval.Collections;
using Pixeval.Controls.Illustrate;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Download;
using Pixeval.Misc;

namespace Pixeval.Pages.Download;

public class DownloadListEntryDataProvider : DataProvider<Illustration, DownloadListEntryViewModel>
{
    public override AdvancedObservableCollection<DownloadListEntryViewModel> View { get; } = [];

    public override IncrementalLoadingCollection<FetchEngineIncrementalSource<Illustration, DownloadListEntryViewModel>, DownloadListEntryViewModel> Source { get; protected set; } = null!;

    public override IFetchEngine<Illustration?>? FetchEngine { get; protected set; }

    public override void DisposeCurrent()
    {
        if (Source is { } source)
            foreach (var downloadListEntryViewModel in source)
                downloadListEntryViewModel.Dispose();

        View.Clear();
    }

    public void ResetAndFillAsync(IEnumerable<ObservableDownloadTask> source)
    {
        Source = new(new DownloadListEntryIncrementalSource(source));
    }

    [Obsolete]
    public override void ResetEngineAsync(IFetchEngine<Illustration?>? fetchEngine, int limit = -1)
    {
        throw new NotImplementedException("DownloadListEntryDataProvider 不使用 FetchEngine");
    }
}
