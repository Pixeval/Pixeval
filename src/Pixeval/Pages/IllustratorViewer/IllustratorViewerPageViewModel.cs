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
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.AppManagement;
using Pixeval.Controls.Windowing;
using Pixeval.Util;
using Pixeval.Util.ComponentModels;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Windows.System;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Controls;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Pages.Capability;
using WinUI3Utilities;
using Symbol = FluentIcons.Common.Symbol;

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

    public NavigationViewTag<BookmarksPage, long> BookmarksTag { get; }

    public NavigationViewTag<FollowingsPage, long> FollowingsTag { get; }

    public NavigationViewTag<MyPixivUsersPage, long> MyPixivUserTag { get; }

    public NavigationViewTag<RelatedUsersPage, long> RelatedUserTag { get; }

    public IllustratorViewerPageViewModel(PixivSingleUserResponse userDetail, ulong hWnd) : base(hWnd)
    {
        UserDetail = userDetail;
        IsFollowed = userDetail.UserEntity.IsFollowed;
        Metrics = userDetail.UserProfile;

        WorkTag = new(Id);
        BookmarksTag = new(Id);
        FollowingsTag = new(Id);
        MyPixivUserTag = new(Id);
        RelatedUserTag = new(Id);

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
            : await AppInfo.PixivNoProfile.ValueAsync;
        if (BackgroundUrl is not null)
        {
            var result2 = await App.AppViewModel.MakoClient.DownloadBitmapImageAsync(BackgroundUrl);
            BackgroundSource = result2 is Result<ImageSource>.Success { Value: var background }
                ? background
                : await AppInfo.ImageNotAvailable.ValueAsync;
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

        HWnd.SuccessGrowl(EntryItemResources.LinkCopiedToClipboard);
    }

    private void GenerateWebLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        UiHelper.ClipboardSetText( MakoHelper.GenerateUserWebUri(Id).OriginalString);
        (args.Parameter as ulong?)?.SuccessGrowl(EntryItemResources.LinkCopiedToClipboard);
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
