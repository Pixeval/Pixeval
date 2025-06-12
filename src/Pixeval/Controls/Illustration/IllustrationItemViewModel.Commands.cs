// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.IO;
using System.Threading.Tasks;
using FluentIcons.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Misaki;
using Pixeval.Download;
using Pixeval.Util;
using Pixeval.Util.UI;

namespace Pixeval.Controls;


public partial class IllustrationItemViewModel
{
    /// <inheritdoc cref="WorkEntryViewModel{T}.SaveCommand"/>
    public XamlUICommand MangaSaveCommand { get; } = EntryItemResources.MangaSave.GetCommand(Symbol.SaveImage);

    /// <inheritdoc cref="WorkEntryViewModel{T}.SaveAsCommand"/>
    public XamlUICommand MangaSaveAsCommand { get; } = EntryItemResources.MangaSaveAs.GetCommand(Symbol.SaveEdit);

    protected override async void SaveCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        await SaveUtilityAsync(args.Parameter as FrameworkElement, App.AppViewModel.AppSettings.DownloadPathMacro);
    }

    protected override async void SaveAsCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        // 必须有Window来显示Picker
        if (args.Parameter is not FrameworkElement frameworkElement)
            return;

        var folder = await frameworkElement.OpenFolderPickerAsync();
        if (folder is null)
        {
            frameworkElement.InfoGrowl(EntryItemResources.SaveAsCancelled);
            return;
        }

        var name = Path.GetFileName(App.AppViewModel.AppSettings.DownloadPathMacro);
        var path = Path.Combine(folder.Path, name);
        await SaveUtilityAsync(frameworkElement, path);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="frameworkElement">承载提示<see cref="Growl"/>的控件，为<see langword="null"/>则不显示</param>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    private async ValueTask SaveUtilityAsync(FrameworkElement? frameworkElement, string path)
    {
        if (IsPicGif && Entry is ISingleAnimatedImage { MultiImageUris: not null } animatedImage)
            await animatedImage.MultiImageUris.TryPreloadListAsync(animatedImage);
        var factory = App.AppViewModel.AppServiceProvider.GetRequiredService<IllustrationDownloadTaskFactory>();
        var task = factory.Create(Entry, path);
        App.AppViewModel.DownloadManager.QueueTask(task);
        frameworkElement?.SuccessGrowl(EntryItemResources.DownloadTaskCreated);
    }
}
