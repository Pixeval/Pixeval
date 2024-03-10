#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustratorPageViewModel.cs
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
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.AppManagement;
using Pixeval.Controls.MarkupExtensions;
using Pixeval.Controls.Windowing;
using Pixeval.Util;
using Pixeval.Util.ComponentModels;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Windows.System;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Pages.Capability;
using WinUI3Utilities;

namespace Pixeval.Pages.IllustratorViewer;

public partial class IllustratorViewerPageViewModel : UiObservableObject
{
    [ObservableProperty]
    private ImageSource? _avatarSource;

    [ObservableProperty]
    private ImageSource? _backgroundSource;

    [ObservableProperty]
    private bool _isFollowed;

    public NavigationViewTag<IllustratorWorkPage, long> WorkTag { get; }

    public NavigationViewTag<BookmarksPage, long> BookmarkedIllustrationAndMangaTag { get; }

    public NavigationViewTag BookmarkedNovelTag { get; }

    public NavigationViewTag<FollowingsPage, long> FollowingUserTag { get; }

    public NavigationViewTag MyPixivUserTag { get; }

    public NavigationViewTag<RecommendUsersPage, long> RecommendUserTag { get; }

    public IllustratorViewerPageViewModel(PixivSingleUserResponse userDetail, FrameworkElement content) : base(content)
    {
        UserDetail = userDetail;
        IsFollowed = userDetail.UserEntity.IsFollowed;
        Metrics = userDetail.UserProfile;

        WorkTag = new(Id);
        BookmarkedIllustrationAndMangaTag = new(Id);
        FollowingUserTag = new(Id);
        MyPixivUserTag = null!;
        BookmarkedNovelTag = null!;
        RecommendUserTag = new(Id);

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

    public string Comment => UserDetail.UserEntity.Comment;

    public async Task SetAvatarAndBackgroundAsync()
    {
        var result = await App.AppViewModel.MakoClient.DownloadBitmapImageAsync(AvatarUrl, 100);
        AvatarSource = result is Result<ImageSource>.Success { Value: var avatar }
            ? avatar
            : await AppInfo.GetPixivNoProfileImageAsync();
        if (BackgroundUrl is not null)
        {
            var result2 = await App.AppViewModel.MakoClient.DownloadBitmapImageAsync(BackgroundUrl);
            BackgroundSource = result2 is Result<ImageSource>.Success { Value: var background }
                ? background
                : await AppInfo.GetNotAvailableImageAsync();
        }
        else
        {
            BackgroundSource = AvatarSource;
        }
    }

    #region Commands

    public XamlUICommand FollowCommand { get; } = XamlUiCommandHelper.GetNewFollowCommand(false);

    public XamlUICommand UnfollowCommand { get; } = XamlUiCommandHelper.GetNewFollowCommand(true);

    public XamlUICommand FollowPrivatelyCommand { get; } = XamlUiCommandHelper.GetNewFollowPrivatelyCommand();

    public XamlUICommand GenerateLinkCommand { get; } = EntryItemResources.GenerateLink.GetCommand(FontIconSymbol.LinkE71B);

    public XamlUICommand GenerateWebLinkCommand { get; } = EntryItemResources.GenerateWebLink.GetCommand(FontIconSymbol.PreviewLinkE8A1);

    public XamlUICommand OpenInWebBrowserCommand { get; } = EntryItemResources.OpenInWebBrowser.GetCommand(FontIconSymbol.WebSearchF6FA);

    public XamlUICommand ShowQrCodeCommand { get; } = EntryItemResources.ShowQRCode.GetCommand(FontIconSymbol.QRCodeED14);

    /// <summary>
    /// 还没用到
    /// </summary>
    public XamlUICommand ShowPixEzQrCodeCommand { get; } = EntryItemResources.ShowPixEzQrCode.GetCommand(FontIconSymbol.Photo2EB9F);

    private void InitializeCommands()
    {
        FollowCommand.ExecuteRequested += async (_, _) => IsFollowed = await MakoHelper.SetFollowAsync(Id, true);

        FollowPrivatelyCommand.ExecuteRequested += async (_, _) => IsFollowed = await MakoHelper.SetFollowAsync(Id, true, true);

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
        UiHelper.ClipboardSetText(MakoHelper.GenerateUserWebUri(Id).OriginalString);
        (args.Parameter as FrameworkElement)?.ShowTeachingTipAndHide(EntryItemResources.LinkCopiedToClipboard);
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

    #endregion
}
