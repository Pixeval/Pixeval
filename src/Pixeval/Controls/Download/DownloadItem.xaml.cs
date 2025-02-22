// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.IO;
using Windows.Foundation;
using Windows.System;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Pixeval.Util.UI;
using WinUI3Utilities;
using Symbol = FluentIcons.Common.Symbol;

namespace Pixeval.Controls;

public sealed partial class DownloadItem
{
    [GeneratedDependencyProperty]
    public partial DownloadItemViewModel ViewModel { get; set; }

    public event Action<DownloadItem, DownloadItemViewModel>? ViewModelChanged;

    public event TypedEventHandler<DownloadItem, DownloadItemViewModel>? OpenIllustrationRequested;

    partial void OnViewModelPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        ViewModelChanged?.Invoke(this, ViewModel);
    }

    public DownloadItem() => InitializeComponent();

    private async void ActionButton_OnClicked(object sender, RoutedEventArgs e)
    {
        switch (ViewModel.ActionButtonSymbol(ViewModel.DownloadTask.CurrentState))
        {
            case Symbol.Dismiss:
                ViewModel.DownloadTask.Cancel();
                break;
            case Symbol.Pause:
                ViewModel.DownloadTask.Pause();
                break;
            case Symbol.ArrowRepeatAll:
                ViewModel.DownloadTask.TryReset();
                break;
            case Symbol.Open:
                if (!await Launcher.LaunchUriAsync(new Uri(ViewModel.DownloadTask.OpenLocalDestination)))
                    _ = await this.CreateAcknowledgementAsync(MiscResources.DownloadItemOpenFailed, MiscResources.DownloadItemMaybeDeleted);
                break;
            case Symbol.Play:
                ViewModel.DownloadTask.TryResume();
                break;
            default:
                ThrowHelper.ArgumentOutOfRange(ViewModel.DownloadTask.CurrentState);
                break;
        }
    }

    private void RedownloadItem_OnClicked(object sender, RoutedEventArgs e)
    {
        ViewModel.DownloadTask.TryReset();
    }

    private void CancelDownloadItem_OnClicked(object sender, RoutedEventArgs e)
    {
        ViewModel.DownloadTask.Cancel();
    }

    private async void OpenDownloadLocationItem_OnClicked(object sender, RoutedEventArgs e)
    {
        _ = await Launcher.LaunchFolderPathAsync(Path.GetDirectoryName(ViewModel.DownloadTask.OpenLocalDestination));
    }

    private void GoToPageItem_OnClicked(object sender, RoutedEventArgs e) => OpenIllustrationRequested?.Invoke(this, ViewModel);

    private async void CheckErrorMessageInDetail_OnClicked(object sender, RoutedEventArgs e)
    {
        _ = await this.CreateAcknowledgementAsync(DownloadItemResources.ErrorMessageDialogTitle, ViewModel.DownloadTask.ErrorCause?.ToString());
    }
}
