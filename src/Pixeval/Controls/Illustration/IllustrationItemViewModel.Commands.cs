#region Copyright
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/IllustrationItemViewModel.Commands.cs
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
using System.Collections.Generic;
using Microsoft.UI.Xaml.Input;
using Pixeval.Util.UI;
using Pixeval.Util;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Utilities;
using System.Threading.Tasks;
using System.IO;
using Pixeval.Download;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Download.Models;
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
        var hWnd = null as ulong?;
        var getImageStreamAsync = null as GetImageStream;
        switch (args.Parameter)
        {
            case (ulong h, GetImageStream f):
                hWnd = h;
                getImageStreamAsync = f;
                break;
            case ulong h:
                hWnd = h;
                break;
            case null:
                break;
            default:
                return;
        }

        await SaveUtilityAsync(hWnd, getImageStreamAsync, App.AppViewModel.AppSettings.DownloadPathMacro);
    }

    protected override async void SaveAsCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        ulong hWnd;
        var getImageStreamAsync = null as GetImageStream;
        switch (args.Parameter)
        {
            case (ulong h, GetImageStream f):
                hWnd = h;
                getImageStreamAsync = f;
                break;
            case ulong h:
                hWnd = h;
                break;
            default:
                // 必须有Window来显示Picker
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
        await SaveUtilityAsync(hWnd, getImageStreamAsync, path);
    }

    /// <summary>
    /// <see cref="IllustrationDownloadTaskFactory"/>
    /// </summary>
    /// <param name="hWnd">承载提示<see cref="TeachingTip"/>的控件，为<see langword="null"/>则不显示</param>
    /// <param name="getImageStreamAsync">获取原图的<see cref="Stream"/>，支持进度显示，为<see langword="null"/>则创建新的下载任务</param>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    private async Task SaveUtilityAsync(ulong? hWnd, GetImageStream? getImageStreamAsync, string path)
    {
        var ib = hWnd?.InfoGrowlReturn("");
        var progress = null as Progress<double>;
        if (ib is not null)
            if (IsUgoira)
                progress = new Progress<double>(d => ib.Title = EntryItemResources.UgoiraProcessing.Format((int)d));
            else
                ib.Title = EntryItemResources.ImageProcessing;

        var source = getImageStreamAsync is null ? null : await getImageStreamAsync.Invoke(progress, true);
        var factory = App.AppViewModel.AppServiceProvider.GetRequiredService<IDownloadTaskFactory<IllustrationItemViewModel, IllustrationDownloadTask>>();
        if (source is null)
        {
            var task = await factory.CreateAsync(this, path);
            App.AppViewModel.DownloadManager.QueueTask(task);
            hWnd?.RemoveSuccessGrowlAfterDelay(ib!, EntryItemResources.DownloadTaskCreated);
        }
        else
        {
            var task = factory.CreateIntrinsic(this, IsUgoira ? (source, UgoiraMetadata) : source, path);
            App.AppViewModel.DownloadManager.QueueTask(task);
            hWnd?.RemoveSuccessGrowlAfterDelay(ib!, EntryItemResources.Saved);
        }
    }

    protected override async void CopyCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        var hWnd = null as ulong?;
        GetImageStream getImageStreamAsync;
        switch (args.Parameter)
        {
            case (ulong h, GetImageStream f):
                hWnd = h;
                getImageStreamAsync = f;
                break;
            case GetImageStream f:
                getImageStreamAsync = f;
                break;
            default:
                return;
        }

        var ib = hWnd?.InfoGrowlReturn("");

        var progress = null as Progress<double>;
        if (ib is not null)
            if (IsUgoira)
                progress = new Progress<double>(d => ib.Title = EntryItemResources.UgoiraProcessing.Format(d));
            else
                ib.Title = EntryItemResources.ImageProcessing;
        if (await getImageStreamAsync(progress, false) is { } source)
        {
            await UiHelper.ClipboardSetBitmapAsync(source);
            hWnd?.RemoveSuccessGrowlAfterDelay(ib!, EntryItemResources.ImageSetToClipBoard);
        }
    }

    public override Uri AppUri => MakoHelper.GenerateIllustrationAppUri(Id);

    public override Uri WebUri => MakoHelper.GenerateIllustrationWebUri(Id);

    public override Uri PixEzUri => MakoHelper.GenerateIllustrationPixEzUri(Id);
}

public delegate Task<Stream?> GetImageStream(IProgress<double>? progress, bool needOriginal);
