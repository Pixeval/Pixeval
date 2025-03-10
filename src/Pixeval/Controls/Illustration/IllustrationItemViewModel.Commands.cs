// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.Download;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
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
        var hWnd = null as FrameworkElement;
        GetImageStreams? getImageStream = null;
        switch (args.Parameter)
        {
            case (FrameworkElement h, GetImageStreams f):
                hWnd = h;
                getImageStream = f;
                break;
            case FrameworkElement h:
                hWnd = h;
                break;
            case null:
                break;
            default:
                return;
        }

        await SaveUtilityAsync(hWnd, getImageStream, App.AppViewModel.AppSettings.DownloadPathMacro);
    }

    protected override async void SaveAsCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        FrameworkElement frameworkElement;
        GetImageStreams? getImageStream = null;
        switch (args.Parameter)
        {
            case (FrameworkElement h, GetImageStreams f):
                frameworkElement = h;
                getImageStream = f;
                break;
            case FrameworkElement h:
                frameworkElement = h;
                break;
            default:
                // 必须有Window来显示Picker
                return;
        }

        var folder = await frameworkElement.OpenFolderPickerAsync();
        if (folder is null)
        {
            frameworkElement.InfoGrowl(EntryItemResources.SaveAsCancelled);
            return;
        }

        var name = Path.GetFileName(App.AppViewModel.AppSettings.DownloadPathMacro);
        var path = Path.Combine(folder.Path, name);
        await SaveUtilityAsync(frameworkElement, getImageStream, path);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="frameworkElement">承载提示<see cref="TeachingTip"/>的控件，为<see langword="null"/>则不显示</param>
    /// <param name="getImageStream">获取原图的<see cref="Stream"/>，为<see langword="null"/>则创建新的下载任务</param>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    private async Task SaveUtilityAsync(FrameworkElement? frameworkElement, GetImageStreams? getImageStream, string path)
    {
        var ib = frameworkElement?.InfoGrowlReturn("");
        if (ib is not null)
            ib.Title = EntryItemResources.ImageProcessing;

        var source = getImageStream is null ? null : await getImageStream(true);

        var factory = App.AppViewModel.AppServiceProvider.GetRequiredService<IllustrationDownloadTaskFactory>();
        if (source is null)
        {
            var task = factory.Create(this, path);
            App.AppViewModel.DownloadManager.QueueTask(task);
            frameworkElement?.RemoveSuccessGrowlAfterDelay(ib!, EntryItemResources.DownloadTaskCreated);
        }
        else
        {
            var task = factory.CreateIntrinsic(this, path, IsUgoira ? (source, await UgoiraMetadata) : source);
            App.AppViewModel.DownloadManager.QueueTask(task);
            frameworkElement?.RemoveSuccessGrowlAfterDelay(ib!, EntryItemResources.Saved);
        }
    }

    protected override async void CopyCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        var hWnd = null as FrameworkElement;
        GetImageStreams getImageStream;
        switch (args.Parameter)
        {
            case (FrameworkElement h, GetImageStreams f):
                hWnd = h;
                getImageStream = f;
                break;
            case GetImageStreams f:
                getImageStream = f;
                break;
            default:
                return;
        }

        var ib = hWnd?.InfoGrowlReturn("");

        Progress<double>? progress = null;
        if (ib is not null)
            if (!IsUgoira)
                progress = new Progress<double>(d => ib.Title = EntryItemResources.UgoiraProcessing.Format(d));
            else
                ib.Title = EntryItemResources.ImageProcessing;
        if (await getImageStream(App.AppViewModel.AppSettings.BrowseOriginalImage) is { } sources)
        {
            if (sources is [var src])
            {
                await UiHelper.ClipboardSetBitmapAsync(src);
                hWnd?.RemoveSuccessGrowlAfterDelay(ib!, EntryItemResources.ImageSetToClipBoard);
                return;
            }
            var source = await sources.UgoiraSaveToStreamAsync((await UgoiraMetadata).Delays.ToArray(), null, progress);
            await UiHelper.ClipboardSetBitmapAsync(source);
            hWnd?.RemoveSuccessGrowlAfterDelay(ib!, EntryItemResources.ImageSetToClipBoard);
        }
    }

    public override Uri AppUri => MakoHelper.GenerateIllustrationAppUri(Id);

    public override Uri WebUri => MakoHelper.GenerateIllustrationWebUri(Id);

    public override Uri PixEzUri => MakoHelper.GenerateIllustrationPixEzUri(Id);
}

public delegate ValueTask<IReadOnlyList<Stream>?> GetImageStreams(bool needOriginal);
