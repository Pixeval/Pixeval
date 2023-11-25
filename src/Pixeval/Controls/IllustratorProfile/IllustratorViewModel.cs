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

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Controls.Illustrate;
using Pixeval.Controls.MarkupExtensions;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Controls;

public sealed partial class IllustratorViewModel : IllustrateViewModel<User>
{
    // Dominant color of the "No Image" image
    public static readonly SolidColorBrush DefaultAvatarBorderColorBrush = new(UiHelper.ParseHexColor("#D6DEE5"));

    public IllustratorViewModel(User user) : base(user)
    {
        IsFollowed = Illustrate.UserInfo?.IsFollowed ?? false;

        SetAvatarAsync().Discard();
        SetBannerSourceAsync().Discard();

        FollowCommand = FollowText.GetCommand(MakoHelper.GetFollowButtonIcon(IsFollowed));
        InitializeCommands();
    }

    public PixivSingleUserResponse? UserDetail { get; private set; }

    public List<SoftwareBitmapSource> BannerSources { get; } = new(3);

    [ObservableProperty]
    private SoftwareBitmapSource? _backgroundSource;

    [ObservableProperty]
    private ImageSource? _avatarSource;

    [ObservableProperty]
    private Brush? _avatarBorderBrush;

    public string Username => Illustrate.UserInfo?.Name ?? "";

    public string UserId => Illustrate.UserInfo?.Id.ToString() ?? "";

    [ObservableProperty]
    private bool _isFollowed;

    public XamlUICommand FollowCommand { get; set; }

    public XamlUICommand ShareCommand { get; set; } = IllustratorProfileResources.Share.GetCommand(FontIconSymbols.ShareE72D);

    public string GetIllustrationToolTipSubtitleText(User? user)
    {
        return user?.UserInfo?.Comment ?? IllustratorProfileResources.UserHasNoComment;
    }

    private async Task SetAvatarAsync()
    {
        var url = Illustrate.UserInfo?.ProfileImageUrls?.Medium;
        if (url is not null)
        {
            var avatar = await App.AppViewModel.MakoClient.DownloadBitmapImageResultAsync(url, 100);
            AvatarSource = avatar.UnwrapOrElse(await AppContext.GetPixivNoProfileImageAsync());
        }
        else
            AvatarSource = await AppContext.GetPixivNoProfileImageAsync();
    }

    private async Task SetBannerSourceAsync()
    {
        var client = App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi);
        AvatarBorderBrush = null;
        DisposeAllBanner();

        if (Illustrate.Illusts is not null)
            foreach (var illustration in Illustrate.Illusts)
            {
                if (illustration.GetThumbnailUrl(ThumbnailUrlOption.SquareMedium) is not { } url)
                    continue;
                if (await client.DownloadAsIRandomAccessStreamAsync(url) is not
                    Result<IRandomAccessStream>.Success(var stream))
                    continue;
                if (AvatarBorderBrush is null)
                {
                    var dominantColor = await UiHelper.GetDominantColorAsync(stream.AsStreamForRead(), false);
                    AvatarBorderBrush = new SolidColorBrush(dominantColor);
                }

                var bitmapSource = await stream.GetSoftwareBitmapSourceAsync(true);
                BannerSources.Add(bitmapSource);

                // 一般只会取 ==
                if (BannerSources.Count >= 3)
                    break;
            }

        OnPropertyChanged(nameof(BannerSources));

        if (AvatarBorderBrush is not null)
            return;

        UserDetail = await App.AppViewModel.MakoClient.GetUserFromIdAsync(UserId, App.AppViewModel.AppSetting.TargetFilter);
        if (UserDetail.UserProfile?.BackgroundImageUrl is { } backgroundImageUrl)
            if (await client.DownloadAsIRandomAccessStreamAsync(backgroundImageUrl) is Result<IRandomAccessStream>.Success(var stream))
            {
                if (AvatarBorderBrush is null)
                {
                    var dominantColor = await UiHelper.GetDominantColorAsync(stream.AsStreamForRead(), false);
                    AvatarBorderBrush = new SolidColorBrush(dominantColor);
                }

                BackgroundSource = await stream.GetSoftwareBitmapSourceAsync(true);
            }

        if (AvatarBorderBrush is not null)
            return;

        AvatarBorderBrush = DefaultAvatarBorderColorBrush;
        BackgroundSource = await AppContext.GetPixivNoProfileImageAsync();
    }

    // private void SetMetrics()
    // {
    //     var followings = UserDetail!.UserProfile?.TotalFollowUsers ?? 0;
    //     var myPixivUsers = UserDetail.UserProfile?.TotalMyPixivUsers ?? 0;
    //     var illustrations = UserDetail.UserProfile?.TotalIllusts ?? 0;
    //     Metrics = new UserMetrics(followings, myPixivUsers, illustrations);
    // }

    private string FollowText => IsFollowed ? IllustratorProfileResources.Unfollow : IllustratorProfileResources.Follow;

    private void InitializeCommands()
    {
        FollowCommand.ExecuteRequested += FollowCommandOnExecuteRequested;
        // TODO: ShareCommand
    }

    private void FollowCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        SwitchFollowState();
        FollowCommand.Label = IsFollowed ? IllustratorProfileResources.Unfollow : IllustratorProfileResources.Follow;
        FollowCommand.IconSource = MakoHelper.GetFollowButtonIcon(IsFollowed);
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
        _ = App.AppViewModel.MakoClient.PostFollowUserAsync(UserId, PrivacyPolicy.Public);
    }

    private void Unfollow()
    {
        IsFollowed = false;
        _ = App.AppViewModel.MakoClient.RemoveFollowUserAsync(UserId);
    }

    public void DisposeAllBanner()
    {
        foreach (var softwareBitmapSource in BannerSources)
            softwareBitmapSource.Dispose();
        BannerSources.Clear();
    }

    public override void Dispose()
    {
        DisposeAllBanner();
    }
}
