// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Mako.Net.Response;
using Pixeval.Pages.Capability;
using Pixeval.Util;
using Pixeval.Util.ComponentModels;
using Pixeval.Util.IO;
using Pixeval.Util.IO.Caching;
using Pixeval.Util.UI;
using Windows.System;
using WinUI3Utilities;
using Symbol = FluentIcons.Common.Symbol;

namespace Pixeval.Pages.IllustratorViewer;

public partial class IllustratorViewerPageViewModel : UiObservableObject
{
    [ObservableProperty]
    public partial ImageSource? AvatarSource { get; set; }

    [ObservableProperty]
    public partial ImageSource? BackgroundSource { get; set; }

    [ObservableProperty]
    public partial bool IsFollowed { get; set; }

    public NavigationViewTag<IllustratorWorkPage> WorkTag { get; } =
        new(EntryViewerPageResources.WorkNavigationViewItemContent) { Symbol = Symbol.Image };

    public NavigationViewTag<BookmarksPage> BookmarksTag { get; } =
        new(EntryViewerPageResources.BookmarksNavigationViewItemContent) { Symbol = Symbol.Library };

    public NavigationViewTag<FollowingsPage> FollowingsTag { get; } =
        new(EntryViewerPageResources.FollowingsNavigationViewItemContent) { Symbol = Symbol.PersonHeart };

    public NavigationViewTag<MyPixivUsersPage> MyPixivUserTag { get; } =
        new(EntryViewerPageResources.MyPixivUserNavigationViewItemContent) { Symbol = Symbol.People };

    public NavigationViewTag<RelatedUsersPage> RelatedUserTag { get; } =
        new(EntryViewerPageResources.RelatedUserNavigationViewItemContent) { Symbol = Symbol.PeopleCommunity };

    public IReadOnlyList<NavigationViewTag> Tags =>
    [
        WorkTag,
        BookmarksTag,
        FollowingsTag,
        MyPixivUserTag,
        RelatedUserTag
    ];

    public IllustratorViewerPageViewModel(PixivSingleUserResponse userDetail, FrameworkElement frameworkElement) : base(frameworkElement)
    {
        UserDetail = userDetail;
        IsFollowed = userDetail.UserEntity.IsFollowed;
        Metrics = userDetail.UserProfile;
        WorkTag.Parameter = BookmarksTag.Parameter = FollowingsTag.Parameter = MyPixivUserTag.Parameter = RelatedUserTag.Parameter = Id;

        InitializeCommands();
        _ = SetAvatarAndBackgroundAsync();
    }

    public PixivSingleUserResponse UserDetail { get; }

    public string Name => UserDetail.UserEntity.Name;

    public Profile Metrics { get; }

    private string AvatarUrl => UserDetail.UserEntity.ProfileImageUrls.Medium;

    private string? BackgroundUrl => UserDetail.UserProfile.BackgroundImageUrl;

    public long Id => UserDetail.UserEntity.Id;

    public string Account => UserDetail.UserEntity.Account;

    public bool IsPremium => UserDetail.UserProfile.IsPremium;

    public string Comment => UserDetail.UserEntity.Description;

    public async Task SetAvatarAndBackgroundAsync()
    {
        AvatarSource = await CacheHelper.GetSourceFromCacheAsync(AvatarUrl, desiredWidth: 100);
        BackgroundSource = BackgroundUrl is not null
            ? await CacheHelper.GetSourceFromCacheAsync(BackgroundUrl)
            : AvatarSource;
    }

    #region Commands

    public XamlUICommand FollowCommand { get; } = XamlUiCommandHelper.GetNewFollowCommand(false);

    public XamlUICommand UnfollowCommand { get; } = XamlUiCommandHelper.GetNewFollowCommand(true);

    public XamlUICommand FollowPrivatelyCommand { get; } = XamlUiCommandHelper.GetNewFollowPrivatelyCommand();

    public XamlUICommand GenerateLinkCommand { get; } = EntryItemResources.GenerateLink.GetCommand(Symbol.Link);

    public XamlUICommand GenerateWebLinkCommand { get; } = EntryItemResources.GenerateWebLink.GetCommand(Symbol.LinkMultiple);

    public XamlUICommand OpenInWebBrowserCommand { get; } = EntryItemResources.OpenInWebBrowser.GetCommand(Symbol.GlobeArrowUp);

    public XamlUICommand ShowQrCodeCommand { get; } = EntryItemResources.ShowQRCode.GetCommand(Symbol.QrCode);

    /// <summary>
    /// 还没用到
    /// </summary>
    public XamlUICommand ShowPixEzQrCodeCommand { get; } = EntryItemResources.ShowPixEzQrCode.GetCommand(Symbol.Image);

    private void InitializeCommands()
    {
        FollowCommand.CanExecuteRequested += (_, e) => e.CanExecute = Id != App.AppViewModel.PixivUid;
        FollowCommand.ExecuteRequested += async (_, _) => IsFollowed = await MakoHelper.SetFollowAsync(Id, true);

        FollowPrivatelyCommand.CanExecuteRequested += (_, e) => e.CanExecute = Id != App.AppViewModel.PixivUid;
        FollowPrivatelyCommand.ExecuteRequested += async (_, _) => IsFollowed = await MakoHelper.SetFollowAsync(Id, true, true);

        UnfollowCommand.CanExecuteRequested += (_, e) => e.CanExecute = Id != App.AppViewModel.PixivUid;
        UnfollowCommand.ExecuteRequested += async (_, _) => IsFollowed = await MakoHelper.SetFollowAsync(Id, false);

        GenerateLinkCommand.ExecuteRequested += GenerateLinkCommandOnExecuteRequested;

        GenerateWebLinkCommand.ExecuteRequested += GenerateWebLinkCommandOnExecuteRequested;

        OpenInWebBrowserCommand.ExecuteRequested += OpenInWebBrowserCommandOnExecuteRequested;

        ShowQrCodeCommand.ExecuteRequested += ShowQrCodeCommandExecuteRequested;

        ShowPixEzQrCodeCommand.ExecuteRequested += ShowPixEzQrCodeCommandExecuteRequested;
    }

    private void GenerateLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        UiHelper.ClipboardSetText(MakoHelper.GenerateUserAppUri(Id).OriginalString);

        FrameworkElement.SuccessGrowl(EntryItemResources.LinkCopiedToClipboard);
    }

    private void GenerateWebLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        UiHelper.ClipboardSetText(MakoHelper.GenerateUserWebUri(Id).OriginalString);
        (args.Parameter as FrameworkElement)?.SuccessGrowl(EntryItemResources.LinkCopiedToClipboard);
    }

    private async void OpenInWebBrowserCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        _ = await Launcher.LaunchUriAsync(MakoHelper.GenerateUserWebUri(Id));
    }

    private async void ShowQrCodeCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (args.Parameter is not TeachingTip showQrCodeTeachingTip)
            return;

        var qrCodeSource = await IoHelper.GenerateQrCodeForUrlAsync(MakoHelper.GenerateUserWebUri(Id).OriginalString);
        ShowQrCodeCommandExecuteRequested(showQrCodeTeachingTip, qrCodeSource);
    }

    private async void ShowPixEzQrCodeCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (args.Parameter is not TeachingTip showQrCodeTeachingTip)
            return;

        var qrCodeSource = await IoHelper.GenerateQrCodeAsync(MakoHelper.GenerateUserPixEzUri(Id).OriginalString);
        ShowQrCodeCommandExecuteRequested(showQrCodeTeachingTip, qrCodeSource);
    }

    private static void ShowQrCodeCommandExecuteRequested(TeachingTip teachingTip, ImageSource source)
    {
        teachingTip.HeroContent.To<Image>().Source = source;
        teachingTip.IsOpen = true;
    }

    #endregion
}
