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
using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Pixeval.Util.UI;
using Pixeval.Util;
using WinUI3Utilities;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Utilities;
using System.Threading.Tasks;
using System.IO;
using Pixeval.Download;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Download.Models;

namespace Pixeval.Controls;

public partial class IllustrationItemViewModel
{
    protected override async void BookmarkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        IsBookmarked = await MakoHelper.SetIllustrationBookmarkAsync(Id, !IsBookmarked);
        BookmarkCommand.GetBookmarkCommand(IsBookmarked);
    }

    protected override async void SaveCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        var frameworkElement = null as FrameworkElement;
        var getOriginalImageSourceAsync = null as Func<IProgress<int>?, Task<Stream?>>;
        switch (args.Parameter)
        {
            case ValueTuple<FrameworkElement?, Func<IProgress<int>?, Task<Stream?>>?> tuple:
                frameworkElement = tuple.Item1;
                getOriginalImageSourceAsync = tuple.Item2;
                break;
            case FrameworkElement f:
                frameworkElement = f;
                break;
        }

        await SaveUtilityAsync(frameworkElement, getOriginalImageSourceAsync, App.AppViewModel.AppSettings.DefaultDownloadPathMacro);
    }

    protected override async void SaveAsCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        Window window;
        var getOriginalImageSourceAsync = null as Func<IProgress<int>?, Task<Stream?>>;
        switch (args.Parameter)
        {
            case ValueTuple<Window, Func<IProgress<int>?, Task<Stream?>>?> tuple:
                window = tuple.Item1;
                getOriginalImageSourceAsync = tuple.Item2;
                break;
            case Window w:
                window = w;
                break;
            default:
                // 必须有Window来显示Picker
                return;
        }

        var frameworkElement = window.Content.To<FrameworkElement>();
        var folder = await window.OpenFolderPickerAsync();
        if (folder is null)
        {
            frameworkElement.ShowTeachingTipAndHide(EntryItemResources.SaveAsCancelled, TeachingTipSeverity.Information);
            return;
        }

        var name = Path.GetFileName(App.AppViewModel.AppSettings.DefaultDownloadPathMacro);
        var path = Path.Combine(folder.Path, name);
        await SaveUtilityAsync(frameworkElement, getOriginalImageSourceAsync, path);
    }

    /// <summary>
    /// <see cref="IllustrationDownloadTaskFactory"/>
    /// </summary>
    /// <param name="frameworkElement">承载提示<see cref="TeachingTip"/>的控件，为<see langword="null"/>则不显示</param>
    /// <param name="getOriginalImageSourceAsync">获取原图的<see cref="IRandomAccessStream"/>，支持进度显示，为<see langword="null"/>则创建新的下载任务</param>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    private async Task SaveUtilityAsync(FrameworkElement? frameworkElement, Func<IProgress<int>?, Task<Stream?>>? getOriginalImageSourceAsync, string path)
    {
        var teachingTip = frameworkElement?.CreateTeachingTip();

        var progress = null as Progress<int>;
        if (IsUgoira)
            progress = new Progress<int>(d => teachingTip?.Show(EntryItemResources.UgoiraProcessing.Format(d), TeachingTipSeverity.Processing, isLightDismissEnabled: true));
        else
            teachingTip?.Show(EntryItemResources.ImageProcessing, TeachingTipSeverity.Processing, isLightDismissEnabled: true);

        var source = getOriginalImageSourceAsync is null ? null : await getOriginalImageSourceAsync.Invoke(progress);
        using var scope = App.AppViewModel.AppServicesScope;
        var factory = scope.ServiceProvider.GetRequiredService<IDownloadTaskFactory<IllustrationItemViewModel, IllustrationDownloadTask>>();
        if (source is null)
        {
            var task = await factory.CreateAsync(this, path);
            App.AppViewModel.DownloadManager.QueueTask(task);
            teachingTip?.ShowAndHide(EntryItemResources.DownloadTaskCreated);
        }
        else
        {
            var task = await factory.TryCreateIntrinsicAsync(this, source, path);
            App.AppViewModel.DownloadManager.QueueTask(task);
            teachingTip?.ShowAndHide(EntryItemResources.Saved);
        }
    }

    protected override async void CopyCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        var frameworkElement = null as FrameworkElement;
        Func<IProgress<int>?, Task<Stream?>> getOriginalImageSourceAsync;
        switch (args.Parameter)
        {
            case ValueTuple<FrameworkElement?, Func<IProgress<int>?, Task<Stream?>>> tuple:
                frameworkElement = tuple.Item1;
                getOriginalImageSourceAsync = tuple.Item2;
                break;
            case Func<IProgress<int>?, Task<Stream?>> f:
                getOriginalImageSourceAsync = f;
                break;
            default:
                return;
        }

        var teachingTip = frameworkElement?.CreateTeachingTip();

        var progress = null as Progress<int>;
        if (IsUgoira)
            progress = new(d => teachingTip?.Show(EntryItemResources.UgoiraProcessing.Format(d), TeachingTipSeverity.Processing, isLightDismissEnabled: true));
        else
            teachingTip?.Show(EntryItemResources.ImageProcessing, TeachingTipSeverity.Processing, isLightDismissEnabled: true);
        if (await getOriginalImageSourceAsync(progress) is { } source)
        {
            await UiHelper.ClipboardSetBitmapAsync(source);
            teachingTip?.ShowAndHide(EntryItemResources.ImageSetToClipBoard);
        }
    }

    protected override Uri AppUri => MakoHelper.GenerateIllustrationAppUri(Id);

    protected override Uri WebUri => MakoHelper.GenerateIllustrationWebUri(Id);

    protected override Uri PixEzUri => MakoHelper.GenerateIllustrationPixEzUri(Id);
}
