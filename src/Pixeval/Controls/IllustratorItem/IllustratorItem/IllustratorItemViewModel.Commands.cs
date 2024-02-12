using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Pixeval.Controls.MarkupExtensions;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Windows.System;
using WinUI3Utilities;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Pixeval.Controls;

public partial class IllustratorItemViewModel
{
    public XamlUICommand FollowCommand { get; } = "".GetCommand(FontIconSymbols.ContactE77B);

    public XamlUICommand GenerateLinkCommand { get; } = IllustrateItemResources.GenerateLink.GetCommand(FontIconSymbols.LinkE71B);

    public XamlUICommand GenerateWebLinkCommand { get; } = IllustrateItemResources.GenerateWebLink.GetCommand(FontIconSymbols.PreviewLinkE8A1);

    public XamlUICommand OpenInWebBrowserCommand { get; } = IllustrateItemResources.OpenInWebBrowser.GetCommand(FontIconSymbols.WebSearchF6FA);

    public XamlUICommand ShowQrCodeCommand { get; } = IllustrateItemResources.ShowQRCode.GetCommand(FontIconSymbols.QRCodeED14);

    public XamlUICommand ShowPixEzQrCodeCommand { get; } = IllustrateItemResources.ShowPixEzQrCode.GetCommand(FontIconSymbols.Photo2EB9F);

    private void InitializeCommands()
    {
        FollowCommand.GetFollowCommand(IsFollowed);
        FollowCommand.ExecuteRequested += FollowCommandExecuteRequested;

        GenerateLinkCommand.ExecuteRequested += GenerateLinkCommandOnExecuteRequested;

        GenerateWebLinkCommand.ExecuteRequested += GenerateWebLinkCommandOnExecuteRequested;

        OpenInWebBrowserCommand.ExecuteRequested += OpenInWebBrowserCommandOnExecuteRequested;

        ShowQrCodeCommand.ExecuteRequested += ShowQrCodeCommandExecuteRequested;

        ShowPixEzQrCodeCommand.ExecuteRequested += ShowPixEzQrCodeCommandExecuteRequested;
    }

    private void FollowCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        IsFollowed = MakoHelper.SetFollow(UserId, !IsFollowed);
        FollowCommand.GetFollowCommand(IsFollowed);
    }

    private void GenerateLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        UiHelper.ClipboardSetText(MakoHelper.GenerateIllustrationAppUri(UserId).OriginalString);

        if (args.Parameter is not TeachingTip teachingTip)
            return;
        if (App.AppViewModel.AppSetting.DisplayTeachingTipWhenGeneratingAppLink)
            teachingTip.IsOpen = true;
        else
            teachingTip?.ShowTeachingTipAndHide(IllustrateItemResources.LinkCopiedToClipboard);
    }

    private void GenerateWebLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        UiHelper.ClipboardSetText(MakoHelper.GenerateIllustratorWebUri(UserId).OriginalString);
        (args.Parameter as FrameworkElement)?.ShowTeachingTipAndHide(IllustrateItemResources.LinkCopiedToClipboard);
    }

    private async void OpenInWebBrowserCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        _ = await Launcher.LaunchUriAsync(MakoHelper.GenerateIllustratorWebUri(UserId));
    }

    private async void ShowQrCodeCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (args.Parameter is not TeachingTip showQrCodeTeachingTip)
            return;

        var qrCodeSource = await IoHelper.GenerateQrCodeForUrlAsync(MakoHelper.GenerateIllustratorWebUri(UserId).OriginalString);
        ShowQrCodeCommandExecuteRequested(showQrCodeTeachingTip, qrCodeSource);
    }

    private async void ShowPixEzQrCodeCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (args.Parameter is not TeachingTip showQrCodeTeachingTip)
            return;

        var qrCodeSource = await IoHelper.GenerateQrCodeAsync(MakoHelper.GenerateIllustratorPixEzUri(UserId).OriginalString);
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
}
