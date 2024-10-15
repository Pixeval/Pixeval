#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/DownloadItem.xaml.cs
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
using Microsoft.UI.Xaml;
using Pixeval.Util.UI;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;
using Symbol = FluentIcons.Common.Symbol;

namespace Pixeval.Controls;

[DependencyProperty<DownloadItemViewModel>("ViewModel", propertyChanged: nameof(OnViewModelChanged))]
public sealed partial class DownloadItem
{
    public event Action<DownloadItem, DownloadItemViewModel>? ViewModelChanged;

    public event TypedEventHandler<DownloadItem, DownloadItemViewModel>? OpenIllustrationRequested;

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d as DownloadItem is { } entry) 
            entry.ViewModelChanged?.Invoke(entry, entry.ViewModel);
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
        _ = await Launcher.LaunchFolderPathAsync(Path.GetDirectoryName(ViewModel.DownloadTask.Destination));
    }

    private void GoToPageItem_OnClicked(object sender, RoutedEventArgs e) => OpenIllustrationRequested?.Invoke(this, ViewModel);

    private async void CheckErrorMessageInDetail_OnClicked(object sender, RoutedEventArgs e)
    {
        _ = await this.CreateAcknowledgementAsync(DownloadItemResources.ErrorMessageDialogTitle, ViewModel.DownloadTask.ErrorCause?.ToString());
    }
}
