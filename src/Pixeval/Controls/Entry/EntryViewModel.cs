// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Windows.System;
using Misaki;
using WinUI3Utilities;
using Symbol = FluentIcons.Common.Symbol;

namespace Pixeval.Controls;

[DebuggerDisplay("{Entry}")]
public abstract class EntryViewModel<T>(T entry) : ObservableObject, IDisposable where T : IMisakiModel
{
    public T Entry { get; } = entry;

    public abstract void Dispose();

    protected void InitializeCommandsBase()
    {
        GenerateLinkCommand.ExecuteRequested += GenerateLinkCommandOnExecuteRequested;

        GenerateWebLinkCommand.ExecuteRequested += GenerateWebLinkCommandOnExecuteRequested;

        OpenInWebBrowserCommand.ExecuteRequested += OpenInWebBrowserCommandOnExecuteRequested;

        ShowQrCodeCommand.ExecuteRequested += ShowQrCodeCommandOnExecuteRequested;

        ShowPixEzQrCodeCommand.ExecuteRequested += ShowPixEzQrCodeCommandOnExecuteRequested;
    }

    public abstract Uri AppUri { get; }

    public abstract Uri WebsiteUri { get; }

    public virtual Uri? PixEzUri => null;

    public XamlUICommand GenerateLinkCommand { get; } = EntryItemResources.GenerateLink.GetCommand(Symbol.Link);

    public XamlUICommand GenerateWebLinkCommand { get; } = EntryItemResources.GenerateWebLink.GetCommand(Symbol.LinkMultiple);

    public XamlUICommand OpenInWebBrowserCommand { get; } = EntryItemResources.OpenInWebBrowser.GetCommand(Symbol.GlobeArrowUp);

    public XamlUICommand ShowQrCodeCommand { get; } = EntryItemResources.ShowQRCode.GetCommand(Symbol.QrCode);

    public XamlUICommand ShowPixEzQrCodeCommand { get; } = EntryItemResources.ShowPixEzQrCode.GetCommand(Symbol.Image);

    private void GenerateLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        UiHelper.ClipboardSetText(AppUri.OriginalString);

        (args.Parameter as FrameworkElement)?.SuccessGrowl(EntryItemResources.LinkCopiedToClipboard);
    }

    private void GenerateWebLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        UiHelper.ClipboardSetText(WebsiteUri.OriginalString);
        (args.Parameter as FrameworkElement)?.SuccessGrowl(EntryItemResources.LinkCopiedToClipboard);
    }

    private async void OpenInWebBrowserCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        _ = await Launcher.LaunchUriAsync(WebsiteUri);
    }

    private async void ShowQrCodeCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (args.Parameter is not TeachingTip showQrCodeTeachingTip)
            return;

        var qrCodeSource = await IoHelper.GenerateQrCodeForUrlAsync(WebsiteUri.OriginalString);
        ShowQrCodeCommandExecuteRequested(showQrCodeTeachingTip, qrCodeSource);
    }

    private async void ShowPixEzQrCodeCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (PixEzUri is null || args.Parameter is not TeachingTip showQrCodeTeachingTip)
            return;

        var qrCodeSource = await IoHelper.GenerateQrCodeAsync(PixEzUri.OriginalString);
        ShowQrCodeCommandExecuteRequested(showQrCodeTeachingTip, qrCodeSource);
    }

    private static void ShowQrCodeCommandExecuteRequested(TeachingTip teachingTip, ImageSource source)
    {
        teachingTip.HeroContent.To<Image>().Source = source;
        teachingTip.IsOpen = true;
    }
}
