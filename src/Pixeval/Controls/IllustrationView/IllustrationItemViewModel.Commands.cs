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
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Pixeval.Controls.MarkupExtensions;
using Pixeval.Util.UI;
using Pixeval.Util;
using WinUI3Utilities;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Utilities;
using System.Threading.Tasks;
using System.IO;
using Pixeval.Download;
using Pixeval.Util.IO;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Download.Models;
using Pixeval.CoreApi.Global.Enum;

namespace Pixeval.Controls.IllustrationView;

public partial class IllustrationItemViewModel
{
    public XamlUICommand BookmarkCommand { get; } = "".GetCommand(
        FontIconSymbols.HeartEB51, VirtualKeyModifiers.Control, VirtualKey.D);

    public XamlUICommand AddToBookmarkCommand { get; } = IllustrationViewerPageResources.AddToBookmark.GetCommand(FontIconSymbols.BookmarksE8A4);

    public XamlUICommand GenerateLinkCommand { get; } = IllustrationViewerPageResources.GenerateLink.GetCommand(FontIconSymbols.LinkE71B);

    public XamlUICommand GenerateWebLinkCommand { get; } = IllustrationViewerPageResources.GenerateWebLink.GetCommand(FontIconSymbols.PreviewLinkE8A1);

    public XamlUICommand OpenInWebBrowserCommand { get; } = IllustrationViewerPageResources.OpenInWebBrowser.GetCommand(FontIconSymbols.WebSearchF6FA);

    public XamlUICommand ShowQrCodeCommand { get; } = IllustrationViewerPageResources.ShowQRCode.GetCommand(FontIconSymbols.QRCodeED14);

    public XamlUICommand ShowPixEzQrCodeCommand { get; } = IllustrationViewerPageResources.ShowPixEzQrCode.GetCommand(FontIconSymbols.Photo2EB9F);

    /// <summary>
    /// Parameter1: <see cref="ValueTuple{T1,T2}"/> where T1 is <see cref="FrameworkElement"/>? and T2 is <see cref="Func{T, TResult}"/>? where T is <see cref="IProgress{T}"/>? and TResult is <see cref="Stream"/>?<br/>
    /// Parameter2: <see cref="FrameworkElement"/>?
    /// </summary>
    public XamlUICommand SaveCommand { get; } = IllustrationViewerPageResources.Save.GetCommand(FontIconSymbols.SaveE74E, VirtualKeyModifiers.Control, VirtualKey.S);

    /// <summary>
    /// Parameter1: <see cref="ValueTuple{T1,T2}"/> where T1 is <see cref="Window"/> and T2 is <see cref="Func{T, TResult}"/>? where T is <see cref="IProgress{T}"/>? and TResult is <see cref="Stream"/>?<br/>
    /// Parameter2: <see cref="Window"/>
    /// </summary>
    public XamlUICommand SaveAsCommand { get; } = IllustrationViewerPageResources.SaveAs.GetCommand(FontIconSymbols.SaveAsE792, VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift, VirtualKey.S);

    /// <summary>
    /// Parameter1: <see cref="ValueTuple{T1,T2}"/> where T1 is <see cref="FrameworkElement"/>? and T2 is <see cref="Func{T, TResult}"/> where T is <see cref="IProgress{T}"/>? and TResult is <see cref="Stream"/>?<br/>
    /// Parameter2: <see cref="Func{T, TResult}"/> where T is <see cref="IProgress{T}"/>? and TResult is <see cref="Stream"/>?
    /// </summary>
    public XamlUICommand CopyCommand { get; } = IllustrationViewerPageResources.Copy.GetCommand(FontIconSymbols.CopyE8C8, VirtualKeyModifiers.Control, VirtualKey.C);

    private void InitializeCommands()
    {
        BookmarkCommand.Label = IsBookmarked ? MiscResources.RemoveBookmark : MiscResources.AddBookmark;
        BookmarkCommand.IconSource = MakoHelper.GetBookmarkButtonIconSource(IsBookmarked);
        BookmarkCommand.ExecuteRequested += BookmarkCommandOnExecuteRequested;

        GenerateLinkCommand.ExecuteRequested += GenerateLinkCommandOnExecuteRequested;

        GenerateWebLinkCommand.ExecuteRequested += GenerateWebLinkCommandOnExecuteRequested;

        OpenInWebBrowserCommand.ExecuteRequested += OpenInWebBrowserCommandOnExecuteRequested;

        ShowQrCodeCommand.ExecuteRequested += ShowQrCodeCommandExecuteRequested;

        ShowPixEzQrCodeCommand.ExecuteRequested += ShowPixEzQrCodeCommandExecuteRequested;

        SaveCommand.ExecuteRequested += SaveCommandExecuteRequested;

        SaveAsCommand.ExecuteRequested += SaveAsCommandExecuteRequested;

        CopyCommand.ExecuteRequested += CopyCommandExecuteRequested;
    }

    private void BookmarkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        _ = IsBookmarked ? RemoveBookmarkAsync() : PostPublicBookmarkAsync();
        // update manually
        BookmarkCommand.Label = IsBookmarked ? MiscResources.RemoveBookmark : MiscResources.AddBookmark;
        BookmarkCommand.IconSource = MakoHelper.GetBookmarkButtonIconSource(IsBookmarked);

        return;
        Task RemoveBookmarkAsync()
        {
            IsBookmarked = false;
            return App.AppViewModel.MakoClient.RemoveBookmarkAsync(Id);
        }

        Task PostPublicBookmarkAsync()
        {
            IsBookmarked = true;
            return App.AppViewModel.MakoClient.PostBookmarkAsync(Id, PrivacyPolicy.Public);
        }
    }

    private void GenerateLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        UiHelper.ClipboardSetText(MakoHelper.GenerateIllustrationAppUri(Id).ToString());

        if (args.Parameter is TeachingTip teachingTip)
            if (App.AppViewModel.AppSetting.DisplayTeachingTipWhenGeneratingAppLink)
                teachingTip.IsOpen = true;
            else
                teachingTip?.ShowTeachingTipAndHide(IllustrationViewerPageResources.LinkCopiedToClipboard);
    }

    private void GenerateWebLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        var link = MakoHelper.GenerateIllustrationWebUri(Id).ToString();
        UiHelper.ClipboardSetText(link);
        (args.Parameter as FrameworkElement)?.ShowTeachingTipAndHide(IllustrationViewerPageResources.LinkCopiedToClipboard);
    }

    private async void OpenInWebBrowserCommandOnExecuteRequested(XamlUICommand xamlUiCommand, ExecuteRequestedEventArgs executeRequestedEventArgs)
    {
        _ = await Launcher.LaunchUriAsync(MakoHelper.GenerateIllustrationWebUri(Id));
    }

    private async void ShowQrCodeCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (args.Parameter is not TeachingTip showQrCodeTeachingTip)
            return;

        var qrCodeSource = await UiHelper.GenerateQrCodeForUrlAsync(MakoHelper.GenerateIllustrationWebUri(Id).ToString());
        ShowQrCodeCommandExecuteRequested(showQrCodeTeachingTip, qrCodeSource);
    }

    private async void ShowPixEzQrCodeCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (args.Parameter is not TeachingTip showQrCodeTeachingTip)
            return;

        var qrCodeSource = await UiHelper.GenerateQrCodeAsync(MakoHelper.GenerateIllustrationPixEzUri(Id).ToString());
        ShowQrCodeCommandExecuteRequested(showQrCodeTeachingTip, qrCodeSource);
    }

    private static void ShowQrCodeCommandExecuteRequested(TeachingTip teachingTip, SoftwareBitmapSource source)
    {
        teachingTip.HeroContent.To<Image>().Source = source;
        teachingTip.IsOpen = true;
        teachingTip.Closed += Closed;
        return;

        void Closed(TeachingTip s, TeachingTipClosedEventArgs ea)
        {
            source.Dispose();
            s.Closed -= Closed;
        }
    }

    private async void SaveCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
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

        var path = App.AppViewModel.AppSetting.DefaultDownloadPathMacro;
        await SaveUtilityAsync(frameworkElement, getOriginalImageSourceAsync, path);
    }

    private async void SaveAsCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
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
            frameworkElement.ShowTeachingTipAndHide("已取消另存为操作", TeachingTipSeverity.Information);
            return;
        }

        var path = Path.Combine(folder.Path, GetName());
        await SaveUtilityAsync(frameworkElement, getOriginalImageSourceAsync, path);
        return;

        // TODO: 名字能使用宏吗
        string GetName()
        {
            string? name;
            if (IsUgoira)
                name = Id + IoHelper.GetUgoiraExtension();
            else if (MangaIndex is -1)
                name = Id + IoHelper.GetIllustrationExtension();
            else
                name = $"{Id}_{MangaIndex}";
            return name;
        }
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
            progress = new(d => teachingTip?.Show(IllustrationViewerPageResources.UgoiraProcessing.Format(d), TeachingTipSeverity.Processing, isLightDismissEnabled: true));
        else
            teachingTip?.Show(IllustrationViewerPageResources.ImageProcessing, TeachingTipSeverity.Processing, isLightDismissEnabled: true);

        var source = getOriginalImageSourceAsync is null ? null : await getOriginalImageSourceAsync.Invoke(progress);
        using var scope = App.AppViewModel.AppServicesScope;
        var factory = scope.ServiceProvider.GetRequiredService<IDownloadTaskFactory<IllustrationItemViewModel, IllustrationDownloadTask>>();
        if (source is null)
        {
            var task = await factory.CreateAsync(this, path);
            App.AppViewModel.DownloadManager.QueueTask(task);
            teachingTip?.ShowAndHide("已创建下载任务");
        }
        else
        {
            var task = await factory.TryCreateIntrinsicAsync(this, source, path);
            App.AppViewModel.DownloadManager.QueueTask(task);
            teachingTip?.ShowAndHide("已保存");
        }
    }

    private async void CopyCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
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
            progress = new(d => teachingTip?.Show(IllustrationViewerPageResources.UgoiraProcessing.Format(d), TeachingTipSeverity.Processing, isLightDismissEnabled: true));
        else
            teachingTip?.Show(IllustrationViewerPageResources.ImageProcessing, TeachingTipSeverity.Processing, isLightDismissEnabled: true);
        if (await getOriginalImageSourceAsync(progress) is { } source)
        {
            UiHelper.ClipboardSetBitmap(source.AsRandomAccessStream());
            teachingTip?.ShowAndHide(IllustrationViewerPageResources.ImageSetToClipBoard);
        }
    }
}
