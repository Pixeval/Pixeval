#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustratorViewModel.cs
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
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using Pixeval.Util.UI;
using Pixeval.CoreApi.Net;
using Windows.Storage.Streams;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Util.Threading;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.UserControls.IllustratorView;

public partial class IllustratorViewModel : ObservableObject, IDisposable
{
    public record UserMetrics(long FollowingCount, long MyPixivUsers /* 好P友 */, long IllustrationCount);

    private readonly User _user;

    public IllustratorViewModel(User user)
    {
        _user = user;
        IsFollowed = _user.UserInfo?.IsFollowed ?? false;
        InitializeAsync().Discard();
    }

    private async Task InitializeAsync()
    {
        UserDetail = await App.AppViewModel.MakoClient.GetUserFromIdAsync(_user.UserInfo?.Id.ToString() ?? string.Empty, App.AppViewModel.AppSetting.TargetFilter);

        SetAvatarAsync().Discard();
        SetBannerSourceAsync().Discard();
        SetMetrics();

        InitializeCommands();
    }

    private async Task SetAvatarAsync()
    {
        var avatar = UserDetail!.UserEntity?.ProfileImageUrls?.Medium?.Let(url => App.AppViewModel.MakoClient.DownloadBitmapImageResultAsync(url, 100)) ?? Task.FromResult(Result<ImageSource>.OfFailure());
        AvatarSource = await avatar.GetOrElseAsync(await AppContext.GetPixivNoProfileImageAsync());
    }

    private async Task SetBannerSourceAsync()
    {
        var stream = await (UserDetail!.UserProfile?.BackgroundImageUrl?.Let(url => App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi).DownloadAsIRandomAccessStreamAsync(url)) ?? Task.FromResult(Result<IRandomAccessStream>.OfFailure()));

        BannerSource = stream switch
        {
            Result<IRandomAccessStream>.Success(var ras) => await Functions.Block(async () =>
            {
                var dominantColor = await UIHelper.GetDominantColorAsync(ras.AsStreamForRead(), false);
                AvatarBorderBrush = new SolidColorBrush(dominantColor);
                return await ras.GetSoftwareBitmapSourceAsync(true);
            }),
            Result<IRandomAccessStream>.Failure => await AppContext.GetPixivNoProfileImageAsync(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void SetMetrics()
    {
        var followings = UserDetail!.UserProfile?.TotalFollowUsers ?? 0;
        var myPixivUsers = UserDetail.UserProfile?.TotalMyPixivUsers ?? 0;
        var illustrations = UserDetail.UserProfile?.TotalIllusts ?? 0;
        Metrics = new UserMetrics(followings, myPixivUsers, illustrations);
    }

    private void InitializeCommands()
    {
        FollowCommand = new XamlUICommand
        {
            Label = IsFollowed ? IllustratorProfileResources.Unfollow : IllustratorProfileResources.Follow,
            IconSource = MakoHelper.GetFollowButtonIcon(UserDetail!.UserEntity?.IsFollowed ?? false)
        };

        FollowCommand.ExecuteRequested += FollowCommandOnExecuteRequested;
        GenerateLinkCommand.ExecuteRequested += GenerateLinkCommandOnExecuteRequested;
        GenerateWebLinkCommand.ExecuteRequested += GenerateWebLinkCommandOnExecuteRequested;
    }

    [ObservableProperty]
    private PixivSingleUserResponse? _userDetail;

    [ObservableProperty]
    private SoftwareBitmapSource? _bannerSource;

    [ObservableProperty]
    private ImageSource? _avatarSource;

    [ObservableProperty]
    private UserMetrics? _metrics;

    [ObservableProperty]
    private Brush? _avatarBorderBrush;

    public string Username => _user.UserInfo?.Name ?? string.Empty;

    public string UserId => _user.UserInfo?.Id.ToString() ?? string.Empty;

    [ObservableProperty]
    private bool _isFollowed;

    [ObservableProperty]
    private XamlUICommand? _followCommand;

    public XamlUICommand ShareCommand { get; } = new()
    {
        Label = IllustratorProfileResources.Share,
        IconSource = FontIconSymbols.ShareE72D.GetFontIconSource()
    };

    public XamlUICommand GenerateLinkCommand { get; } = new()
    {
        Label = IllustratorProfileResources.GenerateLink,
        IconSource = FontIconSymbols.LinkE71B.GetFontIconSource()
    };

    public XamlUICommand GenerateWebLinkCommand { get; } = new()
    {
        Label = IllustratorProfileResources.GenerateWebLink,
        IconSource = FontIconSymbols.PreviewLinkE8A1.GetFontIconSource()
    };

    private void GenerateLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        UIHelper.SetClipboardContent(package => package.SetText(MakoHelper.GenerateIllustratorAppUri(UserId).ToString()));
        SnackBarController.ShowSnack(IllustratorProfileResources.LinkCopiedToClipboard, SnackBarController.SnackBarDurationShort);
    }

    private void GenerateWebLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        UIHelper.SetClipboardContent(package => package.SetText(MakoHelper.GenerateIllustratorWebUri(UserId).ToString()));
        SnackBarController.ShowSnack(IllustratorProfileResources.LinkCopiedToClipboard, SnackBarController.SnackBarDurationShort);
    }

    private void FollowCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (IsFollowed)
            Unfollow();
        else 
            Follow();
    }

    private void Follow()
    {
        IsFollowed = true;
        App.AppViewModel.MakoClient.PostFollowUserAsync(UserId, PrivacyPolicy.Public);
    }

    private void Unfollow()
    {
        IsFollowed = false;
        App.AppViewModel.MakoClient.RemoveFollowUserAsync(UserId);
    }

    public void Dispose()
    {
        BannerSource?.Dispose();
    }
}