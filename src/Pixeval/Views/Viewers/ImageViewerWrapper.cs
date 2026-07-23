// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Avalonia.Input;
using Pixeval.Controls;
using Pixeval.Models.Options;
using Pixeval.Utilities;
using Pixeval.ViewModels.Viewers;

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

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (KeyboardShortcut.TryExecuteCopy(e, ImageViewer.CopyCommand)
            || KeyboardShortcut.TryExecutePlatformCommand(e, Key.S, ImageViewer.SaveCommand))
            return;

        if (ImageViewer.DataContext is not ImageViewerViewModel { CurrentPage: { } currentPage })
            return;

        if (KeyboardShortcut.TryExecutePlatformCommand(
                e,
                Key.S,
                currentPage.SaveAsCommand,
                this,
                KeyModifiers.Shift))
            return;

        if (KeyboardShortcut.Matches(e, Key.Space)
            && currentPage.PlayPauseCommand.CanExecute(null))
        {
            currentPage.IsPlaying = !currentPage.IsPlaying;
            e.Handled = true;
        }
    }

    public event EventHandler<Control, ImageViewerSelectionChangedEventArgs>? SelectionChanged
    {
        add => ImageViewer.SelectionChanged += value;
        remove => ImageViewer.SelectionChanged -= value;
    }
}
