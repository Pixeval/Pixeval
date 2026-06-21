// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Pixeval.Controls;
using Pixeval.Models.Options;

namespace Pixeval.Views.Viewers;

public class ImageViewerWrapper : ContentControl
{
    public ImageViewerBase ImageViewer => (ImageViewerBase) Content!;

    public bool IsSwipeMode { get; } = App.AppViewModel.AppSettings.BrowsingExperienceSettings.BrowseMode is BrowseMode.Swipe;

    public SwipeImageViewer? SwipeImageViewer => Content as SwipeImageViewer;

    public ImageViewerWrapper()
    {
        DataContext = null;
        ImageViewerBase imageViewer = IsSwipeMode
            ? new SwipeImageViewer()
            : new ContinuousImageViewer();
        imageViewer.BrowseDirection = App.AppViewModel.AppSettings.BrowsingExperienceSettings.BrowseDirection;
        Content = imageViewer;
    }

    public event EventHandler<Control, ImageViewerSelectionChangedEventArgs>? SelectionChanged
    {
        add => ImageViewer.SelectionChanged += value;
        remove => ImageViewer.SelectionChanged -= value;
    }
}
