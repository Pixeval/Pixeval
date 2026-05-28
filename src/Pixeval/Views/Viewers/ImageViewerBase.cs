// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.Input;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Models.Options;
using Pixeval.Utilities;
using Pixeval.ViewModels.Viewers;

namespace Pixeval.Views.Viewers;

public abstract partial class ImageViewerBase : UserControl
{
    public event EventHandler<Control, ImageViewerSelectionChangedEventArgs>? SelectionChanged;

    public static readonly DirectProperty<ImageViewerBase, bool> IsPlayingProperty =
        AvaloniaProperty.RegisterDirect<ImageViewerBase, bool>(
            nameof(IsPlaying),
            o => o.IsPlaying,
            (o, v) => o.IsPlaying = v);

    public static readonly DirectProperty<ImageViewerBase, double> ZoomFactorProperty =
        AvaloniaProperty.RegisterDirect<ImageViewerBase, double>(
            nameof(ZoomFactor),
            o => o.ZoomFactor,
            (o, v) => o.ZoomFactor = v);

    public static readonly DirectProperty<ImageViewerBase, BrowseDirection> BrowseDirectionProperty =
        AvaloniaProperty.RegisterDirect<ImageViewerBase, BrowseDirection>(
            nameof(BrowseDirection),
            o => o.BrowseDirection,
            (o, v) => o.BrowseDirection = v);

    public bool IsPlaying
    {
        get;
        set => SetAndRaise(IsPlayingProperty, ref field, value);
    } = true;

    public abstract double ZoomFactor { get; set; }

    public BrowseDirection BrowseDirection
    {
        get;
        set
        {
            var old = field;
            if (old == value)
                return;

            SetAndRaise(BrowseDirectionProperty, ref field, value);
            OnBrowseDirectionChanged(old, value);
        }
    }

    protected SingleViewerViewModel? CurrentPageViewModel { get; private set; }

    protected bool CanManipulateCurrentImage => CurrentPageViewModel?.LoadSuccessfully == true;

    protected bool CanPlayCurrentGif => CurrentPageViewModel?.IsGifLoadSuccessfully == true;

    protected void SetCurrentPageViewModel(SingleViewerViewModel? currentPage)
    {
        if (ReferenceEquals(CurrentPageViewModel, currentPage))
            return;

        CurrentPageViewModel?.PropertyChanged -= CurrentPageOnPropertyChanged;
        CurrentPageViewModel = currentPage;
        CurrentPageViewModel?.PropertyChanged += CurrentPageOnPropertyChanged;

        NotifyCommandCanExecuteChanged();
    }

    protected void RaiseSelectionChanged(int newIndex, object? newItem) =>
        SelectionChanged?.Invoke(this, new ImageViewerSelectionChangedEventArgs(newIndex, newItem));

    protected virtual void NotifyCommandCanExecuteChanged()
    {
        ZoomInCommand.NotifyCanExecuteChanged();
        ZoomOutCommand.NotifyCanExecuteChanged();
        ZoomToOriginalCommand.NotifyCanExecuteChanged();
        PlayPauseCommand.NotifyCanExecuteChanged();
        CopyCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        SaveAsCommand.NotifyCanExecuteChanged();
    }

    protected virtual void OnBrowseDirectionChanged(BrowseDirection oldValue, BrowseDirection newValue)
    {
    }

    private void CurrentPageOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SingleViewerViewModel.LoadSuccessfully)
            or nameof(SingleViewerViewModel.OriginalSource)
            or nameof(SingleViewerViewModel.DisplaySource)
            or nameof(SingleViewerViewModel.IsGifLoadSuccessfully))
            NotifyCommandCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanManipulateCurrentImage))]
    private void ZoomIn() => ZoomFactor *= 1.2;

    [RelayCommand(CanExecute = nameof(CanManipulateCurrentImage))]
    private void ZoomOut() => ZoomFactor /= 1.2;

    [RelayCommand(CanExecute = nameof(CanManipulateCurrentImage))]
    private void ZoomToOriginal() => ZoomFactor = 1;

    [RelayCommand(CanExecute = nameof(CanPlayCurrentGif))]
    private void PlayPause() => IsPlaying = !IsPlaying;

    [RelayCommand(CanExecute = nameof(CanManipulateCurrentImage))]
    private async Task CopyAsync() => await CopyCurrentImageToClipboardAsync();

    [RelayCommand(CanExecute = nameof(CanManipulateCurrentImage))]
    private async Task SaveAsync() => await CopyCurrentImageToClipboardAsync();

    [RelayCommand(CanExecute = nameof(CanManipulateCurrentImage))]
    private async Task SaveAsAsync() => await CopyCurrentImageToClipboardAsync();

    private async Task CopyCurrentImageToClipboardAsync()
    {
        if (CurrentPageViewModel?.DisplaySource?.Frames is not [var singleFrame])
            return;
        if (TopLevel.GetTopLevel(this) is not { ViewContainer: { } viewContainer, Clipboard: { } clipboard })
            return;

        await clipboard.SetBitmapAsync(singleFrame);
        viewContainer?.ShowSuccess(I18NManager.GetResource(MiscResources.Copied));
    }
}
