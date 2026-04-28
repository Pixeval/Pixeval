using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using FluentIcons.Common;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.ViewModels;

namespace Pixeval.Views.Download;

public partial class DownloadItem : UserControl
{
    public event Action<DownloadItem, DownloadItemViewModel>? OpenIllustrationRequested;

    public DownloadItem() => InitializeComponent();

    private DownloadItemViewModel? ViewModel => DataContext as DownloadItemViewModel;

    private async void ActionButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is not { } vm)
            return;

        if (vm.ActionButtonSymbol is Symbol.Open)
            await OpenPathAsync(vm.DownloadTask.OpenLocalDestination);
        else
            vm.ExecutePrimaryAction();
    }

    private void RedownloadItem_OnClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel?.DownloadTask.TryReset();
    }

    private void CancelDownloadItem_OnClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel?.DownloadTask.Cancel();
    }

    private async void OpenDownloadLocationItem_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is not { } vm)
            return;

        var path = Path.GetDirectoryName(vm.DownloadTask.OpenLocalDestination);
        if (!string.IsNullOrWhiteSpace(path))
            await OpenPathAsync(path);
    }

    private void GoToPageItem_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is not { } vm)
            return;

        OpenIllustrationRequested?.Invoke(this, vm);
    }

    private async void CheckErrorMessageInDetail_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is not { } vm)
            return;

        if (TopLevel.GetTopLevel(this)?.ViewContainer is { } viewContainer)
            _ = await viewContainer.CreateAcknowledgementAsync(
                I18NManager.GetResource(DownloadItemResources.ErrorMessageDialogTitle),
                vm.DownloadTask.ErrorCause?.ToString());
    }

    private async Task OpenPathAsync(string path)
    {
        if (TopLevel.GetTopLevel(this) is not { Launcher: { } launcher })
            return;

        try
        {
            var info = new DirectoryInfo(path);
            if (info.Exists)
            {
                _ = await launcher.LaunchDirectoryInfoAsync(info);
                return;
            }

            var file = new FileInfo(path);
            if (file.Exists)
            {
                _ = await launcher.LaunchFileInfoAsync(file);
                return;
            }
            // todo not exists
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowError(
                I18NManager.GetResource(DownloadItemResources.ActionButtonContentOpen),
                path);
        }
        catch
        {
            // TODO: Show error message
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowError(
                I18NManager.GetResource(DownloadItemResources.ActionButtonContentOpen),
                path);
        }
    }
}
