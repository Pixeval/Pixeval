#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/DownloadListEntry.xaml.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.IO;
using Windows.Foundation;
using Windows.System;
using Microsoft.UI.Xaml.Input;
using Pixeval.Controls;
using Pixeval.Download;
using Pixeval.Download.Models;
using Pixeval.Util.UI;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Pages.Download;

[DependencyProperty<DownloadListEntryViewModel>("ViewModel")]
public sealed partial class DownloadListEntry : IViewModelControl
{
    object IViewModelControl.ViewModel => ViewModel;

    public DownloadListEntry() => InitializeComponent();

    public event TypedEventHandler<DownloadListEntry, DownloadListEntryViewModel>? OpenIllustrationRequested;

    private async void ActionButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        switch (ViewModel.DownloadTask.CurrentState)
        {
            case DownloadState.Queued:
                ViewModel.DownloadTask.CancellationHandle.Cancel();
                break;
            case DownloadState.Running:
                ViewModel.DownloadTask.CancellationHandle.Pause();
                break;
            case DownloadState.Error:
            case DownloadState.Cancelled:
            case DownloadState.Completed:
                _ = await (ViewModel.DownloadTask is MangaDownloadTask
                    ? Launcher.LaunchFolderPathAsync(Path.GetDirectoryName(ViewModel.DownloadTask.Destination))
                    : Launcher.LaunchUriAsync(new Uri(ViewModel.DownloadTask.Destination)));
                break;
            case DownloadState.Paused:
                ViewModel.DownloadTask.CancellationHandle.Resume();
                break;
            default:
                ThrowHelper.ArgumentOutOfRange(ViewModel.DownloadTask.CurrentState);
                break;
        }
    }

    private void RedownloadItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        // ViewModel.DownloadTask.Reset();
        // _ = App.AppViewModel.DownloadManager.TryExecuteInline(ViewModel.DownloadTask);
    }

    private void CancelDownloadItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel.DownloadTask.CancellationHandle.Cancel();
    }

    private async void OpenDownloadLocationItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = await Launcher.LaunchFolderPathAsync(Path.GetDirectoryName(ViewModel.DownloadTask.Destination));
    }

    private void GoToPageItem_OnTapped(object sender, TappedRoutedEventArgs e) => OpenIllustrationRequested?.Invoke(this, ViewModel);

    private async void CheckErrorMessageInDetail_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = await this.CreateAcknowledgementAsync(DownloadListEntryResources.ErrorMessageDialogTitle, ViewModel.DownloadTask.ErrorCause?.ToString());
    }
}
