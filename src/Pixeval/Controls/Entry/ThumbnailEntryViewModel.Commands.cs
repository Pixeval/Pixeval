#region Copyright
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/ThumbnailEntryViewModel.Commands.cs
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
using System.IO;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Controls.MarkupExtensions;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using WinUI3Utilities;

namespace Pixeval.Controls;

public partial class ThumbnailEntryViewModel<T>
{
    public XamlUICommand AddToBookmarkCommand { get; } = EntryItemResources.AddToBookmark.GetCommand(FontIconSymbol.BookmarksE8A4);

    public XamlUICommand GenerateLinkCommand { get; } = EntryItemResources.GenerateLink.GetCommand(FontIconSymbol.LinkE71B);

    public XamlUICommand GenerateWebLinkCommand { get; } = EntryItemResources.GenerateWebLink.GetCommand(FontIconSymbol.PreviewLinkE8A1);

    public XamlUICommand OpenInWebBrowserCommand { get; } = EntryItemResources.OpenInWebBrowser.GetCommand(FontIconSymbol.WebSearchF6FA);

    public XamlUICommand ShowQrCodeCommand { get; } = EntryItemResources.ShowQRCode.GetCommand(FontIconSymbol.QRCodeED14);

    public XamlUICommand ShowPixEzQrCodeCommand { get; } = EntryItemResources.ShowPixEzQrCode.GetCommand(FontIconSymbol.Photo2EB9F);

    /// <summary>
    /// Parameter1: <see cref="ValueTuple{T1, T2}"/> where T1 is <see cref="FrameworkElement"/>? and T2 is <see cref="Func{T, TResult}"/>? where T is <see cref="IProgress{T}"/>? and TResult is <see cref="Stream"/>?<br/>
    /// Parameter2: <see cref="FrameworkElement"/>?
    /// </summary>
    public XamlUICommand BookmarkCommand { get; } = "".GetCommand(FontIconSymbol.HeartEB51, VirtualKeyModifiers.Control, VirtualKey.D);

    /// <summary>
    /// Parameter1: <see cref="ValueTuple{T1, T2}"/> where T1 is <see cref="FrameworkElement"/>? and T2 is <see cref="Func{T, TResult}"/>? where T is <see cref="IProgress{T}"/>? and TResult is <see cref="Stream"/>?<br/>
    /// Parameter2: <see cref="FrameworkElement"/>?
    /// </summary>
    public XamlUICommand SaveCommand { get; } = EntryItemResources.Save.GetCommand(FontIconSymbol.SaveE74E, VirtualKeyModifiers.Control, VirtualKey.S);

    /// <summary>
    /// Parameter1: <see cref="ValueTuple{T1,T2}"/> where T1 is <see cref="Window"/> and T2 is <see cref="Func{T, TResult}"/>? where T is <see cref="IProgress{T}"/>? and TResult is <see cref="Stream"/>?<br/>
    /// Parameter2: <see cref="Window"/>
    /// </summary>
    public XamlUICommand SaveAsCommand { get; } = EntryItemResources.SaveAs.GetCommand(FontIconSymbol.SaveAsE792, VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift, VirtualKey.S);

    /// <summary>
    /// Parameter1: <see cref="ValueTuple{T1,T2}"/> where T1 is <see cref="FrameworkElement"/>? and T2 is <see cref="Func{T, TResult}"/> where T is <see cref="IProgress{T}"/>? and TResult is <see cref="Stream"/>?<br/>
    /// Parameter2: <see cref="Func{T, TResult}"/> where T is <see cref="IProgress{T}"/>? and TResult is <see cref="Stream"/>?
    /// </summary>
    public XamlUICommand CopyCommand { get; } = EntryItemResources.Copy.GetCommand(FontIconSymbol.CopyE8C8, VirtualKeyModifiers.Control, VirtualKey.C);

    private void InitializeCommands()
    {
        // TODO: AddToBookmarkCommand
        AddToBookmarkCommand.CanExecuteRequested += (sender, args) => args.CanExecute = false;

        GenerateLinkCommand.ExecuteRequested += GenerateLinkCommandOnExecuteRequested;

        GenerateWebLinkCommand.ExecuteRequested += GenerateWebLinkCommandOnExecuteRequested;

        OpenInWebBrowserCommand.ExecuteRequested += OpenInWebBrowserCommandOnExecuteRequested;

        ShowQrCodeCommand.ExecuteRequested += ShowQrCodeCommandOnExecuteRequested;

        ShowPixEzQrCodeCommand.ExecuteRequested += ShowPixEzQrCodeCommandOnExecuteRequested;

        BookmarkCommand.GetBookmarkCommand(IsBookmarked);
        BookmarkCommand.ExecuteRequested += BookmarkCommandOnExecuteRequested;

        SaveCommand.ExecuteRequested += SaveCommandOnExecuteRequested;

        SaveAsCommand.ExecuteRequested += SaveAsCommandOnExecuteRequested;

        CopyCommand.ExecuteRequested += CopyCommandOnExecuteRequested;
    }

    private void GenerateLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        UiHelper.ClipboardSetText(AppUri.OriginalString);

        if (args.Parameter is TeachingTip teachingTip)
        {
            if (App.AppViewModel.AppSettings.DisplayTeachingTipWhenGeneratingAppLink)
                teachingTip.IsOpen = true;
            else
                teachingTip?.ShowTeachingTipAndHide(EntryItemResources.LinkCopiedToClipboard);
        }
        // 只提示
        else
            (args.Parameter as FrameworkElement)?.ShowTeachingTipAndHide(EntryItemResources.LinkCopiedToClipboard);
    }

    private void GenerateWebLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        UiHelper.ClipboardSetText(WebUri.OriginalString);
        (args.Parameter as FrameworkElement)?.ShowTeachingTipAndHide(EntryItemResources.LinkCopiedToClipboard);
    }

    private async void OpenInWebBrowserCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        _ = await Launcher.LaunchUriAsync(WebUri);
    }

    private async void ShowQrCodeCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (args.Parameter is not TeachingTip showQrCodeTeachingTip)
            return;

        var qrCodeSource = await IoHelper.GenerateQrCodeForUrlAsync(WebUri.OriginalString);
        ShowQrCodeCommandExecuteRequested(showQrCodeTeachingTip, qrCodeSource);
    }

    private async void ShowPixEzQrCodeCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (args.Parameter is not TeachingTip showQrCodeTeachingTip)
            return;

        var qrCodeSource = await IoHelper.GenerateQrCodeAsync(PixEzUri.OriginalString);
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

    protected abstract void BookmarkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args);

    protected abstract void SaveCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args);

    protected abstract void SaveAsCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args);

    protected abstract void CopyCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args);

    protected abstract Uri AppUri { get; }

    protected abstract Uri WebUri { get; }

    protected abstract Uri PixEzUri { get; }
}
