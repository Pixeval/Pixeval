// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Microsoft.UI.Xaml.Input;
using Pixeval.Util;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Pixeval.CoreApi.Model;
using Pixeval.Download;
using Pixeval.Util.UI;

namespace Pixeval.Controls;

public partial class NovelItemViewModel
{
    protected override Task<bool> SetBookmarkAsync(long id, bool isBookmarked, bool privately = false, IEnumerable<string>? tags = null) => MakoHelper.SetNovelBookmarkAsync(id, isBookmarked, privately, tags);

    protected override void SaveCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        var hWnd = null as ulong?;
        DocumentViewerViewModel? documentViewerViewModel = null;
        switch (args.Parameter)
        {
            case (ulong h, DocumentViewerViewModel vm):
                hWnd = h;
                documentViewerViewModel = vm;
                break;
            case ulong h:
                hWnd = h;
                break;
            case null:
                break;
            default:
                return;
        }

        SaveUtility(hWnd, documentViewerViewModel, App.AppViewModel.AppSettings.DownloadPathMacro);
    }

    protected override async void SaveAsCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        ulong hWnd;
        DocumentViewerViewModel? documentViewerViewModel = null;
        switch (args.Parameter)
        {
            case (ulong h, DocumentViewerViewModel vm):
                hWnd = h;
                documentViewerViewModel = vm;
                break;
            case ulong h:
                hWnd = h;
                break;
            // 必须有Window来显示Picker
            default:
                return;
        }

        var folder = await hWnd.OpenFolderPickerAsync();
        if (folder is null)
        {
            hWnd.InfoGrowl(EntryItemResources.SaveAsCancelled);
            return;
        }

        var name = Path.GetFileName(App.AppViewModel.AppSettings.DownloadPathMacro);
        var path = Path.Combine(folder.Path, name);
        SaveUtility(hWnd, documentViewerViewModel, path);
    }

    /// <summary>
    /// <see cref="IllustrationDownloadTaskFactory"/>
    /// </summary>
    /// <param name="hWnd">承载提示<see cref="TeachingTip"/>的控件，为<see langword="null"/>则不显示</param>
    /// <param name="source">为<see langword="null"/>则创建新的下载任务</param>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    private void SaveUtility(ulong? hWnd, DocumentViewerViewModel? source, string path)
    {
        var ib = hWnd?.InfoGrowlReturn(EntryItemResources.NovelContentFetching);

        var factory = App.AppViewModel.AppServiceProvider.GetRequiredService<NovelDownloadTaskFactory>();
        if (source is null)
        {
            var task = factory.Create(this, path);
            App.AppViewModel.DownloadManager.QueueTask(task);
            hWnd?.RemoveSuccessGrowlAfterDelay(ib!, EntryItemResources.DownloadTaskCreated);
        }
        else
        {
            var task = factory.CreateIntrinsic(this, source, path);
            App.AppViewModel.DownloadManager.QueueTask(task);
            hWnd?.RemoveSuccessGrowlAfterDelay(ib!, EntryItemResources.Saved);
        }
    }

    /// <summary>
    /// 此处只会将文章内容设置到剪贴板，所以不需要异步就可以很快结束
    /// </summary>
    protected override void CopyCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        var hWnd = null as ulong?;
        NovelContent? novelContent;
        switch (args.Parameter)
        {
            case (ulong h, NovelContent c):
                hWnd = h;
                novelContent = c;
                break;
            case NovelContent c:
                novelContent = c;
                break;
            default:
                return;
        }

        UiHelper.ClipboardSetText(novelContent.Text);
        hWnd?.SuccessGrowl(EntryItemResources.NovelSetToClipBoard);
    }

    public override Uri AppUri => MakoHelper.GenerateNovelAppUri(Id);

    public override Uri WebUri => MakoHelper.GenerateNovelWebUri(Id);

    public override Uri PixEzUri => MakoHelper.GenerateNovelPixEzUri(Id);
}
