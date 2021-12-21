using System;
using System.Diagnostics;
using System.Linq;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Download;
using Pixeval.Misc;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util.UI;

namespace Pixeval.Pages.Download;

[DependencyProperty("ViewModel", typeof(ObservableDownloadTask), InstanceChangedCallback = true)]
[DependencyProperty("Thumbnail", typeof(ImageSource))]
[DependencyProperty("Title", typeof(string))]
[DependencyProperty("Description", typeof(string))]
[DependencyProperty("Progress", typeof(double))]
[DependencyProperty("ProgressMessage", typeof(string))]
[DependencyProperty("ActionButtonContent", typeof(string))]
[DependencyProperty("IsRedownloadItemEnabled", typeof(bool))]
[DependencyProperty("IsCancelItemEnabled", typeof(bool))]
[DependencyProperty("ActionButtonBackground", typeof(Brush))]
[DependencyProperty("IsShowErrorDetailDialogItemEnabled", typeof(bool))]
public sealed partial class DownloadListEntry
{
    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DownloadListEntry entry && e.NewValue is ObservableDownloadTask value)
        {
            ToolTipService.SetToolTip(entry, value.Title);
            ToolTipService.SetPlacement(entry, PlacementMode.Mouse);
        }
    }
    private EventHandler<bool>? _selected;

    public event EventHandler<bool> Selected
    {
        add => _selected += value;
        remove => _selected -= value;
    }

    public DownloadListEntry()
    {
        InitializeComponent();
    }

    private void DownloadListEntry_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        const int minWindowWidth = 768;
        switch (e.PreviousSize.Width)
        {
            case > minWindowWidth when e.NewSize.Width <= minWindowWidth:
                ApplyVisualStateChange(true);
                break;
            case <= minWindowWidth when e.NewSize.Width > minWindowWidth:
                ApplyVisualStateChange(false);
                break;
        }
    }

    private void ApplyVisualStateChange(bool compact)
    {
        // VisualStateManager won't work for unknown reason, sucks
        if (compact)
        {
            ImageColumn.Width = new GridLength(1, GridUnitType.Star);
            CaptionColumn.Width = new GridLength(0);
            ProgressColumn.Width = new GridLength(0);
            ButtonColumn.Width = new GridLength(0);
            OptionColumn.Width = new GridLength(0);
            Grid.SetRowSpan(ThumbnailImageContainer, 1);
            Grid.SetRowSpan(CaptionContainer, 1);
            Grid.SetColumn(CaptionContainer, 0);
            CaptionContainer.Margin = new Thickness(60, 0, 0, 0);
            CaptionContainer.HorizontalAlignment = HorizontalAlignment.Left;
            Grid.SetRow(ProgressBarContainer, 1);
            Grid.SetRowSpan(ProgressBarContainer, 1);
            Grid.SetColumn(ProgressBarContainer, 0);
            ProgressBarContainer.Margin = new Thickness(0, 8, 0, 0);
            Grid.SetRow(ActionButton, 2);
            Grid.SetRowSpan(ActionButton, 1);
            Grid.SetColumn(ActionButton, 0);
            ActionButton.Width = double.NaN;
            ActionButton.HorizontalAlignment = HorizontalAlignment.Stretch;
            ActionButton.Margin = new Thickness(0, 8, 0, 0);
            Grid.SetRowSpan(MoreOptionButton, 1);
            Grid.SetColumn(MoreOptionButton, 0);
            MoreOptionButton.HorizontalAlignment = HorizontalAlignment.Right;
        }
        else
        {
            ImageColumn.Width = new GridLength(60);
            CaptionColumn.Width = new GridLength(120);
            ProgressColumn.Width = new GridLength(1, GridUnitType.Star);
            ButtonColumn.Width = new GridLength(130);
            OptionColumn.Width = new GridLength(45);
            Grid.SetRowSpan(ThumbnailImageContainer, 3);
            Grid.SetRowSpan(CaptionContainer, 3);
            Grid.SetColumn(CaptionContainer, 1);
            CaptionContainer.Margin = new Thickness(0);
            CaptionContainer.HorizontalAlignment = HorizontalAlignment.Stretch;
            Grid.SetRow(ProgressBarContainer, 0);
            Grid.SetRowSpan(ProgressBarContainer, 3);
            Grid.SetColumn(ProgressBarContainer, 2);
            ProgressBarContainer.Margin = new Thickness(0, 3, 50, 0);
            Grid.SetRow(ActionButton, 0);
            Grid.SetRowSpan(ActionButton, 3);
            Grid.SetColumn(ActionButton, 3);
            ActionButton.Width = 120;
            ActionButton.HorizontalAlignment = HorizontalAlignment.Center;
            ActionButton.Margin = new Thickness(0);
            Grid.SetRowSpan(MoreOptionButton, 3);
            Grid.SetColumn(MoreOptionButton, 4);
            MoreOptionButton.HorizontalAlignment = HorizontalAlignment.Center;
        }
    }

    private async void ActionButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        switch (ViewModel.CurrentState)
        {
            case DownloadState.Created:
            case DownloadState.Queued:
                ViewModel.CancellationHandle.Cancel();
                break;
            case DownloadState.Running:
                ViewModel.CancellationHandle.Pause();
                break;
            case DownloadState.Error:
            case DownloadState.Cancelled:
            case DownloadState.Completed:
                await Launcher.LaunchUriAsync(new Uri(ViewModel.Destination));
                break;
            case DownloadState.Paused:
                ViewModel.CancellationHandle.Resume();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void RedownloadItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel.Reset();
        App.AppViewModel.DownloadManager.TryExecuteInline(ViewModel);
    }

    private void CancelDownloadItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel.CancellationHandle.Cancel();
    }

    private void OpenDownloadLocationItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        Process.Start("explorer.exe", $@"/select, ""{ViewModel.Destination}""");
    }

    private async void GoToPageItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        switch (ViewModel)
        {
            case IIllustrationViewModelProvider provider:
                var viewModels = (await provider.GetViewModelAsync())
                    .GetMangaIllustrationViewModels()
                    .ToArray();

                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", App.AppViewModel.AppWindowRootFrame);
                App.AppViewModel.RootFrameNavigate(typeof(IllustrationViewerPage), new IllustrationViewerPageViewModel(viewModels), new SuppressNavigationTransitionInfo());
                break;
        }
    }

    private async void CheckErrorMessageInDetail_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await MessageDialogBuilder.CreateAcknowledgement(App.AppViewModel.Window, DownloadListEntryResources.ErrorMessageDialogTitle, ViewModel.ErrorCause!.ToString())
            .ShowAsync();
    }

    private void MoreOptionButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
    }

    private void BackgroundGrid_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel.Selected = !ViewModel.Selected;
        _selected?.Invoke(this, ViewModel.Selected);
    }
}