// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Misaki;

namespace Pixeval.ViewModels.Viewers;

public sealed partial class ImageViewerViewModel : ViewModelBase, IDisposable
{
    private bool _isDisposed;

    public ImageViewerViewModel(IllustrationItemViewModel thumbnailViewModel)
    {
        ThumbnailViewModel = thumbnailViewModel;
        var platform = thumbnailViewModel.Entry.Platform;

        var entry = thumbnailViewModel.Entry;

        Images = entry is not IImageSet set
            ? [new(platform, entry, 0)]
            : set.Pages.Select((t, i) => new SingleViewerViewModel(platform, t, i)).ToArray();

        PageCount = Images.Count;

        App.AppViewModel.HistoryPersistHelper.AddBrowseHistory(thumbnailViewModel.Entry);
    }

    public IllustrationItemViewModel ThumbnailViewModel { get; set; }

    public IReadOnlyList<SingleViewerViewModel> Images { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentPage))]
    public partial int SelectedPageIndex { get; set; }

    public SingleViewerViewModel? CurrentPage =>
        Images.Count is 0 ? null : Images[int.Clamp(SelectedPageIndex, 0, Images.Count - 1)];

    public int PageCount { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        foreach (var loadableBitmap in Images)
            loadableBitmap.Dispose();
    }
}
