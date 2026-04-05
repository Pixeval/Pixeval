// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Mako.Model;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.I18N;
using Pixeval.Models.Download;
using Pixeval.Utilities;
using Pixeval.Views.ViewContainers;

namespace Pixeval.ViewModels;

public partial class IllustrationItemViewModel
{
    /// <inheritdoc />
    public override bool IsBookmarkSupported => Entry is Illustration;

    /// <inheritdoc />
    protected override async Task SaveAsync(Control? parameter)
    {
        if (TopLevel.GetTopLevel(parameter) is { ViewContainer: { } viewContainer })
            await SaveInternalAsync(viewContainer, App.AppViewModel.AppSettings.DownloadPathMacro);
    }

    /// <inheritdoc />
    protected override async Task SaveAsAsync(Control? parameter)
    {
        // 必须有TopLevel来显示Picker
        if (TopLevel.GetTopLevel(parameter) is not { ViewContainer: { } viewContainer } topLevel)
            return;

        var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new() { AllowMultiple = false });
        if (folder is not [{ } single])
        {
            viewContainer.ShowInformation(I18NManager.GetResource(EntryItemResources.SaveAsCancelled));
            return;
        }

        var name = Path.GetFileName(App.AppViewModel.AppSettings.DownloadPathMacro);
        var path = Path.Combine(single.Path.OriginalString, name);
        await SaveInternalAsync(viewContainer, path);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="viewContainerBase">承载提示的控件，为<see langword="null"/>则不显示</param>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    private async ValueTask SaveInternalAsync(ViewContainerBase? viewContainerBase, string path)
    {
        if (IsPicGif && Entry is ISingleAnimatedImage { MultiImageUris: not null } animatedImage)
            await animatedImage.MultiImageUris.TryPreloadListAsync(animatedImage);
        var factory = App.AppViewModel.AppServiceProvider.GetRequiredService<IllustrationDownloadTaskFactory>();
        var task = factory.Create(Entry, path);
        App.AppViewModel.DownloadManager.QueueTask(task);
        viewContainerBase?.ShowSuccess(I18NManager.GetResource(EntryItemResources.DownloadTaskCreated));
    }
}
