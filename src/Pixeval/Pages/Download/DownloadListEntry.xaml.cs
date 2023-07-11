#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/DownloadListEntry.xaml.cs
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
using System.Diagnostics;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pixeval.Download;
using Pixeval.Util.UI;
using Windows.System;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Pages.Download;

[DependencyProperty<ObservableDownloadTask>("ViewModel", propertyChanged: nameof(OnViewModelChanged))]
[DependencyProperty<ImageSource>("Thumbnail")]
[DependencyProperty<string>("Title")]
[DependencyProperty<string>("Description")]
[DependencyProperty<double>("Progress")]
[DependencyProperty<string>("ProgressMessage")]
[DependencyProperty<string>("ActionButtonContent")]
[DependencyProperty<bool>("IsRedownloadItemEnabled")]
[DependencyProperty<bool>("IsCancelItemEnabled")]
[DependencyProperty<bool>("IsShowErrorDetailDialogItemEnabled")]
public sealed partial class DownloadListEntry
{
    public DownloadListEntry()
    {
        InitializeComponent();
    }

    private bool IsSelected => ViewModel?.Selected ?? false;

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DownloadListEntry entry && e.NewValue is ObservableDownloadTask value)
        {
            ToolTipService.SetToolTip(entry, value.Title);
            ToolTipService.SetPlacement(entry, PlacementMode.Mouse);
        }
    }

    public event EventHandler<bool>? Selected;

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

                // ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", App.AppViewModel.AppWindowRootFrame);
                // todo UIHelper.RootFrameNavigate(typeof(IllustrationViewerPage), new IllustrationViewerPageViewModel(viewModels), new SuppressNavigationTransitionInfo());
                break;
        }
    }

    private async void CheckErrorMessageInDetail_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await MessageDialogBuilder.CreateAcknowledgement(CurrentContext.Window, DownloadListEntryResources.ErrorMessageDialogTitle, ViewModel.ErrorCause!.ToString())
            .ShowAsync();
    }

    private Brush _lastBorderBrush = (Brush)Application.Current.Resources["SystemControlHighlightAccentBrush"];

    private void RootCardControl_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel.Selected = !ViewModel.Selected;
        (_lastBorderBrush, RootCardControl.BorderBrush) = (RootCardControl.BorderBrush, _lastBorderBrush);
        (RootCardControl.Margin, RootCardControl.BorderThickness) = (RootCardControl.BorderThickness, RootCardControl.Margin);
        Selected?.Invoke(this, ViewModel.Selected);
    }
}
