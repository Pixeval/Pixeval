using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Pixeval.Controls.MarkupExtensions;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Windows.System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using WinUI3Utilities;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.AppManagement;
using Pixeval.CoreApi.Model;
using Pixeval.Utilities;

namespace Pixeval.Controls;

public abstract partial class UserItemViewModel<T>(T illustrate, IllustratorIllustrationsOverviewViewModel overviewViewModel) : EntryViewModel<T>(illustrate) where T : IEntry
{
    [ObservableProperty]
    private bool _isFollowed;

    [ObservableProperty]
    private ImageSource? _avatarSource;

    public abstract long UserId { get; }

    public abstract string AvatarUrl { get; }

    public IllustratorIllustrationsOverviewViewModel OverviewViewModel { get; } = overviewViewModel;

    public async Task LoadAvatarAsync()
    {
        var result = await App.AppViewModel.MakoClient.DownloadBitmapImageAsync(AvatarUrl, 100);
        AvatarSource = result is Result<ImageSource>.Success { Value: var avatar }
            ? avatar
            : await AppInfo.GetPixivNoProfileImageAsync();
        await OverviewViewModel.LoadBannerSource();
    }

    public override void Dispose()
    {
        AvatarSource = null;
        OverviewViewModel.Dispose();
    }

    public XamlUICommand FollowCommand { get; } = "".GetCommand(FontIconSymbol.ContactE77B);

    public XamlUICommand GenerateLinkCommand { get; } = EntryItemResources.GenerateLink.GetCommand(FontIconSymbol.LinkE71B);

    public XamlUICommand GenerateWebLinkCommand { get; } = EntryItemResources.GenerateWebLink.GetCommand(FontIconSymbol.PreviewLinkE8A1);

    public XamlUICommand OpenInWebBrowserCommand { get; } = EntryItemResources.OpenInWebBrowser.GetCommand(FontIconSymbol.WebSearchF6FA);

    public XamlUICommand ShowQrCodeCommand { get; } = EntryItemResources.ShowQRCode.GetCommand(FontIconSymbol.QRCodeED14);

    public XamlUICommand ShowPixEzQrCodeCommand { get; } = EntryItemResources.ShowPixEzQrCode.GetCommand(FontIconSymbol.Photo2EB9F);

    protected void InitializeCommands()
    {
        FollowCommand.GetFollowCommand(IsFollowed);
        FollowCommand.ExecuteRequested += FollowCommandExecuteRequested;

        GenerateLinkCommand.ExecuteRequested += GenerateLinkCommandOnExecuteRequested;

        GenerateWebLinkCommand.ExecuteRequested += GenerateWebLinkCommandOnExecuteRequested;

        OpenInWebBrowserCommand.ExecuteRequested += OpenInWebBrowserCommandOnExecuteRequested;

        ShowQrCodeCommand.ExecuteRequested += ShowQrCodeCommandExecuteRequested;

        ShowPixEzQrCodeCommand.ExecuteRequested += ShowPixEzQrCodeCommandExecuteRequested;
    }

    protected virtual void FollowCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        IsFollowed = MakoHelper.SetFollow(UserId, !IsFollowed);
        FollowCommand.GetFollowCommand(IsFollowed);
    }

    private void GenerateLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        UiHelper.ClipboardSetText(MakoHelper.GenerateIllustrationAppUri(UserId).OriginalString);

        if (args.Parameter is not TeachingTip teachingTip)
            return;
        if (App.AppViewModel.AppSettings.DisplayTeachingTipWhenGeneratingAppLink)
            teachingTip.IsOpen = true;
        else
            teachingTip?.ShowTeachingTipAndHide(EntryItemResources.LinkCopiedToClipboard);
    }

    private void GenerateWebLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        UiHelper.ClipboardSetText(MakoHelper.GenerateIllustratorWebUri(UserId).OriginalString);
        (args.Parameter as FrameworkElement)?.ShowTeachingTipAndHide(EntryItemResources.LinkCopiedToClipboard);
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
