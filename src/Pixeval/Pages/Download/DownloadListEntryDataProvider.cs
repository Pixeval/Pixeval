using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Pixeval.Collections;
using Pixeval.Download.Models;

namespace Pixeval.Pages.Download;

public class DownloadListEntryDataProvider : ObservableObject, IDisposable
{
    public AdvancedObservableCollection<DownloadListEntryViewModel> View { get; } = [];

    public IncrementalLoadingCollection<DownloadListEntryIncrementalSource, DownloadListEntryViewModel> Source
    {
        get => (View.Source as IncrementalLoadingCollection<DownloadListEntryIncrementalSource, DownloadListEntryViewModel>)!;
        protected set => View.Source = value;
    }

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

    public void ResetEngine(IEnumerable<DownloadTaskBase> source)
    {
        Source = new IncrementalLoadingCollection<DownloadListEntryIncrementalSource, DownloadListEntryViewModel>(new DownloadListEntryIncrementalSource(source));
    }
}
