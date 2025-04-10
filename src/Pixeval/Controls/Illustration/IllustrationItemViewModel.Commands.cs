// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Mako.Model;
using Mako.Net.Response;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.Download;
using Pixeval.Util;
using Pixeval.Util.UI;
using Symbol = FluentIcons.Common.Symbol;

namespace Pixeval.Controls;

public partial class IllustrationItemViewModel
{
    /// <inheritdoc cref="WorkEntryViewModel{T}.SaveCommand"/>
    public XamlUICommand MangaSaveCommand { get; } = EntryItemResources.MangaSave.GetCommand(Symbol.SaveImage);

    /// <inheritdoc cref="WorkEntryViewModel{T}.SaveAsCommand"/>
    public XamlUICommand MangaSaveAsCommand { get; } = EntryItemResources.MangaSaveAs.GetCommand(Symbol.SaveEdit);

    protected override Task<bool> SetBookmarkAsync(long id, bool isBookmarked, bool privately = false, IEnumerable<string>? tags = null) => MakoHelper.SetIllustrationBookmarkAsync(id, isBookmarked, privately, tags);

    protected override async void SaveCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        var frameworkElement = null as FrameworkElement;
        switch (args.Parameter)
        {
            case FrameworkElement h:
                frameworkElement = h;
                break;
            case null:
                break;
            default:
                return;
        }

        SaveUtility(frameworkElement, IsUgoira ? await UgoiraMetadataAsync : null, App.AppViewModel.AppSettings.DownloadPathMacro);
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
        SaveUtility(frameworkElement, IsUgoira ? await UgoiraMetadataAsync : null, path);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="frameworkElement">承载提示<see cref="TeachingTip"/>的控件，为<see langword="null"/>则不显示</param>
    /// <param name="ugoiraMetadata"></param>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    private void SaveUtility(FrameworkElement? frameworkElement, UgoiraMetadata? ugoiraMetadata, string path)
    {
        var factory = App.AppViewModel.AppServiceProvider.GetRequiredService<IllustrationDownloadTaskFactory>();
        var task = factory.Create(Entry, path, ugoiraMetadata);
        App.AppViewModel.DownloadManager.QueueTask(task);
        frameworkElement?.SuccessGrowl(EntryItemResources.DownloadTaskCreated);
    }

    public override Uri AppUri => MakoHelper.GenerateIllustrationAppUri(Id);

    public override Uri WebUri => MakoHelper.GenerateIllustrationWebUri(Id);

    public override Uri PixEzUri => MakoHelper.GenerateIllustrationPixEzUri(Id);
}
