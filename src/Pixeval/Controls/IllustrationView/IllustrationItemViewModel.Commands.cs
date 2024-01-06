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

    public XamlUICommand SaveCommand { get; } = IllustrationViewerPageResources.Save.GetCommand(
        FontIconSymbols.SaveE74E, VirtualKeyModifiers.Control, VirtualKey.S);

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

        SaveCommand.ExecuteRequested += SaveAsync;
    }

    private void BookmarkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        _ = ToggleBookmarkStateAsync();
        // update manually
        BookmarkCommand.Label = IsBookmarked ? MiscResources.RemoveBookmark : MiscResources.AddBookmark;
        BookmarkCommand.IconSource = MakoHelper.GetBookmarkButtonIconSource(IsBookmarked);
    }

    private void GenerateLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (App.AppViewModel.AppSetting.DisplayTeachingTipWhenGeneratingAppLink)
            if (args.Parameter is TeachingTip teachingTip)
                teachingTip.IsOpen = true;

        UiHelper.ClipboardSetText(MakoHelper.GenerateIllustrationAppUri(Id).ToString());
    }

    private void GenerateWebLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        var link = MakoHelper.GenerateIllustrationWebUri(Id).ToString();
        UiHelper.ClipboardSetText(link);
        (args.Parameter as FrameworkElement)?.ShowTeachingTipAndHide(IllustrationViewerPageResources.WebLinkCopiedToClipboardToastTitle);
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

    private async void SaveAsync(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (args.Parameter is not ValueTuple<FrameworkElement, Func<IProgress<int>?, Task<IRandomAccessStream?>>>
            {
                Item1: var frameworkElement,
                Item2: var getOriginalImageSourceForClipBoardAsync
            })
            return;

        var teachingTip = frameworkElement.CreateTeachingTip();

        var progress = null as Progress<int>;
        if (IsUgoira)
            progress = new(d => teachingTip.Show(IllustrationViewerPageResources.UgoiraProcessing.Format(d), TeachingTipSeverity.Processing, isLightDismissEnabled: true));
        else
            teachingTip.Show(IllustrationViewerPageResources.ImageProcessing, TeachingTipSeverity.Processing, isLightDismissEnabled: true);
        if (await getOriginalImageSourceForClipBoardAsync(progress) is { } source)
        {
            await SaveAsync(source);
            teachingTip?.ShowAndHide("已保存");
        }
    }
}
