// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.I18N;
using Pixeval.Models.Blocking;
using Pixeval.Models.Download;
using Pixeval.Utilities;
using Pixeval.Views.ViewContainers;

namespace Pixeval.ViewModels;

public partial class IllustrationItemViewModel
{
    [RelayCommand]
    private static async Task CopyAsync(Image? parameter)
    {
        if (parameter is not { Source: Bitmap bitmap })
            return;
        if (TopLevel.GetTopLevel(parameter) is not
            { ViewContainer: { } viewContainer, Clipboard: { } clipboard })
            return;
        await clipboard.SetBitmapAsync(bitmap);
        await clipboard.FlushAsync();
        viewContainer?.ShowSuccess(I18NManager.GetResource(MiscResources.Copied));
    }

    /// <inheritdoc />
    protected override Task SaveAsync(Control? parameter) => SaveAsyncCore(parameter, -1);

    internal Task SaveImageAsync(Control? parameter, int setIndex) => SaveAsyncCore(parameter, setIndex);

    private async Task SaveAsyncCore(Control? parameter, int setIndex)
    {
        if (BlockedContentHelper.IsBlockedPlaceholder(Entry))
            return;

        await SaveInternalAsync(
            TopLevel.GetTopLevel(parameter)?.ViewContainer,
            App.AppViewModel.AppSettings.DownloadSettings.DownloadPathMacro,
            setIndex);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="viewContainerBase">承载提示的控件，为<see langword="null"/>则不显示</param>
    /// <param name="path">文件路径</param>
    /// <param name="setIndex">图集中的图片序号</param>
    /// <returns></returns>
    private async ValueTask SaveInternalAsync(ViewContainerBase? viewContainerBase, string path, int setIndex)
    {
        if (IsPicGif && Entry is ISingleAnimatedImage { MultiImageUris: not null } animatedImage)
            await animatedImage.MultiImageUris.TryPreloadListAsync(animatedImage);
        var factory = App.AppViewModel.AppServiceProvider.GetRequiredService<IllustrationDownloadTaskFactory>();
        var task = factory.Create(Entry, path, setIndex);
        App.AppViewModel.HistoryPersistHelper.DownloadManager.QueueTask(task);
        viewContainerBase?.ShowSuccess(I18NManager.GetResource(EntryItemResources.DownloadTaskCreated));
    }
}
