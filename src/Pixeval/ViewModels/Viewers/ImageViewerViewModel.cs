// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Misaki;
using Pixeval.Models.Database.Managers;

namespace Pixeval.ViewModels.Viewers;

public partial class ImageViewerViewModel : ViewModelBase, IDisposable
{
    public ImageViewerViewModel(IllustrationItemViewModel thumbnailViewModel)
    {
        ThumbnailViewModel = thumbnailViewModel;
        var platform = thumbnailViewModel.Entry.Platform;

        var entry = thumbnailViewModel.Entry;

        Images = entry is not IImageSet set
            ? [new(platform, entry)]
            : set.Pages.Select(t => new SingleViewerViewModel(platform, t)).ToArray();

        PageCount = Images.Count;

        BrowseHistoryPersistentManager.AddHistory(thumbnailViewModel.Entry);
    }

    public IllustrationItemViewModel ThumbnailViewModel { get; set; }

    public IReadOnlyList<SingleViewerViewModel> Images { get; }

    [ObservableProperty]
    public partial int SelectedPageIndex { get; set; }

    public int PageCount { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var loadableBitmap in Images)
            loadableBitmap.Dispose();
    }
}
