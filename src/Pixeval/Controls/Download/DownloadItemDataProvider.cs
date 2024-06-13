using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Pixeval.Collections;
using Pixeval.Download.Models;

namespace Pixeval.Controls;

public partial class DownloadItemDataProvider : ObservableObject, IDisposable
{
    public AdvancedObservableCollection<DownloadItemViewModel> View { get; } = [];

    public IncrementalLoadingCollection<DownloadItemIncrementalSource, DownloadItemViewModel> Source
    {
        get => (View.Source as IncrementalLoadingCollection<DownloadItemIncrementalSource, DownloadItemViewModel>)!;
        protected set => View.Source = value;
    }

    public void Dispose()
    {
        if (Source is { } source)
            foreach (var downloadListEntry in source)
                downloadListEntry.Dispose();

        View.Clear();
    }

    public void ResetEngine(IEnumerable<DownloadTaskBase> source)
    {
        Source = new IncrementalLoadingCollection<DownloadItemIncrementalSource, DownloadItemViewModel>(new DownloadItemIncrementalSource(source));
    }
}
