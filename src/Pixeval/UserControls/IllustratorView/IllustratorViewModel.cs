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
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Model;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using Pixeval.Util.UI;
using Pixeval.CoreApi.Net;
using Windows.Storage.Streams;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Options;
using Pixeval.Util.Threading;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.UserControls.IllustratorView;

public partial class IllustratorViewModel : ObservableObject, IDisposable
{
    // Dominant color of the "No Image" image
    public static readonly SolidColorBrush DefaultAvatarBorderColorBrush = new(UIHelper.ParseHexColor("#D6DEE5"));

    private readonly TaskCompletionSource<SoftwareBitmapSource[]> _bannerImageTaskCompletionSource;

    public IllustratorViewModel(User user)
    {
        User = user;
        _bannerImageTaskCompletionSource = new TaskCompletionSource<SoftwareBitmapSource[]>();
        IsFollowed = User.UserInfo?.IsFollowed ?? false;
        Username = User.UserInfo?.Name ?? string.Empty;
        UserId = User.UserInfo?.Id.ToString() ?? string.Empty;

        SetAvatarAsync().Discard();
        SetBannerSourceAsync().Discard();

        InitializeCommands();
    }

    public PixivSingleUserResponse? UserDetail { get; private set; }

    public Task<SoftwareBitmapSource[]> BannerImageTask => _bannerImageTaskCompletionSource.Task;

    [ObservableProperty]
    private User? _user;

    [ObservableProperty]
    private ImageSource? _avatarSource;

    [ObservableProperty]
    private Brush? _avatarBorderBrush;

    [ObservableProperty]
    private string? _username;

    [ObservableProperty]
    private string? _userId;

    [ObservableProperty]
    private bool _isFollowed;

    [ObservableProperty]
    private XamlUICommand? _followCommand;

    [ObservableProperty]
    private XamlUICommand? _shareCommand;

    [ObservableProperty]
    private XamlUICommand? _generateLinkCommand;

    [ObservableProperty]
    private XamlUICommand? _generateWebLinkCommand;

    public string GetIllustrationToolTipSubtitleText(User? user)
    {
        return user?.UserInfo?.Comment ?? IllustratorProfileResources.UserHasNoComment;
    }

    private async Task SetAvatarAsync()
    {
        var avatar = User!.UserInfo?.ProfileImageUrls?.Medium?.Let(url => App.AppViewModel.MakoClient.DownloadBitmapImageResultAsync(url, 100)) ?? Task.FromResult(Result<ImageSource>.OfFailure());
        AvatarSource = await avatar.GetOrElseAsync(await AppContext.GetPixivNoProfileImageAsync());
    }

    private async Task SetBannerSourceAsync()
    {
        // Try to get user display illustrations 
        var client = App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi);
        if (User!.Illusts?.Take(3).ToArray() is { Length: > 0 } illustrations && illustrations.SelectNotNull(c => c.GetThumbnailUrl(ThumbnailUrlOption.SquareMedium)).ToArray() is [.. var urls])
        {
            var tasks = await Task.WhenAll(urls.Select(u => client.DownloadAsIRandomAccessStreamAsync(u)));
            if (tasks is [Result<IRandomAccessStream>.Success(var first), ..])
            {
                var dominantColor = await UIHelper.GetDominantColorAsync(first.AsStreamForRead(), false);
                AvatarBorderBrush = new SolidColorBrush(dominantColor);
            }
            var result = (await Task.WhenAll(tasks.SelectNotNull(r => r.BindAsync(s => s.GetSoftwareBitmapSourceAsync(true)))))
                .SelectNotNull(res => res is Result<SoftwareBitmapSource>.Success(var sbs) ? sbs : null).ToArray();
            _bannerImageTaskCompletionSource.TrySetResult(result);
            return;
        }

        UserDetail = await App.AppViewModel.MakoClient.GetUserFromIdAsync(User!.UserInfo?.Id.ToString() ?? string.Empty, App.AppViewModel.AppSetting.TargetFilter);
        // otherwise use banner image
        if (UserDetail.UserProfile?.BackgroundImageUrl is { } url && await client.DownloadAsIRandomAccessStreamAsync(url) is Result<IRandomAccessStream>.Success(var stream))
        {
            var managedStream = stream.AsStreamForRead();
            var dominantColor = await UIHelper.GetDominantColorAsync(managedStream, false);
            AvatarBorderBrush = new SolidColorBrush(dominantColor);
            var result = Enumerates.ArrayOf(await stream.GetSoftwareBitmapSourceAsync(true));
            _bannerImageTaskCompletionSource.TrySetResult(result);
            return;
        }

        // if user has no illustrations and no banner image, use default "no profile" image.
        AvatarBorderBrush = DefaultAvatarBorderColorBrush;
        _bannerImageTaskCompletionSource.TrySetResult(Enumerates.ArrayOf(await AppContext.GetPixivNoProfileImageAsync()));
    }

    // private void SetMetrics()
    // {
    //     var followings = UserDetail!.UserProfile?.TotalFollowUsers ?? 0;
    //     var myPixivUsers = UserDetail.UserProfile?.TotalMyPixivUsers ?? 0;
    //     var illustrations = UserDetail.UserProfile?.TotalIllusts ?? 0;
    //     Metrics = new UserMetrics(followings, myPixivUsers, illustrations);
    // }

    private void InitializeCommands()
    {
        FollowCommand = new XamlUICommand
        {
            Label = IsFollowed ? IllustratorProfileResources.Unfollow : IllustratorProfileResources.Follow,
            IconSource = MakoHelper.GetFollowButtonIcon(User!.UserInfo?.IsFollowed ?? false)
        };

        GenerateLinkCommand = new XamlUICommand
        {
            Label = IllustratorProfileResources.GenerateWebLink,
            IconSource = FontIconSymbols.LinkE71B.GetFontIconSource()
        };

        GenerateWebLinkCommand = new XamlUICommand
        {
            Label = IllustratorProfileResources.GenerateWebLink,
            IconSource = FontIconSymbols.PreviewLinkE8A1.GetFontIconSource()
        };

        ShareCommand = new XamlUICommand
        {
            Label = IllustratorProfileResources.Share,
            IconSource = FontIconSymbols.ShareE72D.GetFontIconSource()
        };

        FollowCommand.ExecuteRequested += FollowCommandOnExecuteRequested;
        GenerateLinkCommand.ExecuteRequested += GenerateLinkCommandOnExecuteRequested;
        GenerateWebLinkCommand.ExecuteRequested += GenerateWebLinkCommandOnExecuteRequested;
    }

    private void GenerateLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        UIHelper.SetClipboardContent(package => package.SetText(MakoHelper.GenerateIllustratorAppUri(UserId!).ToString()));
        SnackBarController.ShowSnack(IllustratorProfileResources.LinkCopiedToClipboard, SnackBarController.SnackBarDurationShort);
    }

    private void GenerateWebLinkCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        UIHelper.SetClipboardContent(package => package.SetText(MakoHelper.GenerateIllustratorWebUri(UserId!).ToString()));
        SnackBarController.ShowSnack(IllustratorProfileResources.LinkCopiedToClipboard, SnackBarController.SnackBarDurationShort);
    }

    private void FollowCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        SwitchFollowState();
        FollowCommand!.Label = IsFollowed ? IllustratorProfileResources.Unfollow : IllustratorProfileResources.Follow;
        FollowCommand!.IconSource = MakoHelper.GetFollowButtonIcon(IsFollowed);
    }

    private void SwitchFollowState()
    {
        if (IsFollowed)
            Unfollow();
        else
            Follow();
    }

    private void Follow()
    {
        IsFollowed = true;
        App.AppViewModel.MakoClient.PostFollowUserAsync(UserId!, PrivacyPolicy.Public);
    }

    private void Unfollow()
    {
        IsFollowed = false;
        App.AppViewModel.MakoClient.RemoveFollowUserAsync(UserId!);
    }

    public void Dispose()
    {
        _bannerImageTaskCompletionSource.Task.ContinueWith(s => s.Dispose());
        BannerImageTask.ContinueWith(i => i.Dispose());
    }
}