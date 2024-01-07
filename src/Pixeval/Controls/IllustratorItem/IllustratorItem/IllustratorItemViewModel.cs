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
using Pixeval.CoreApi.Net.Response;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Controls;

public sealed partial class IllustratorItemViewModel : IllustrateViewModel<User>
{
    [ObservableProperty]
    private ImageSource? _avatarSource;

    [ObservableProperty]
    private SoftwareBitmapSource? _backgroundSource;

    [ObservableProperty]
    private bool _isFollowed;

    public IllustratorItemViewModel(User user) : base(user)
    {
        OverviewViewModel = new IllustratorIllustrationsOverviewViewModel(Illustrate.Illusts);
        IsFollowed = Illustrate.UserInfo?.IsFollowed ?? false;

        _ = SetAvatarAsync();
        _ = SetBackgroundAsync();

        FollowCommand = FollowText.GetCommand(MakoHelper.GetFollowButtonIcon(IsFollowed));
        InitializeCommands();
    }

    public IllustratorIllustrationsOverviewViewModel OverviewViewModel { get; }

    public PixivSingleUserResponse? UserDetail { get; private set; }

    public string Username => Illustrate.UserInfo?.Name ?? "";

    public long UserId => Illustrate.UserInfo?.Id ?? 0;

    public XamlUICommand FollowCommand { get; set; }

    public XamlUICommand ShareCommand { get; set; } = IllustratorItemResources.Share.GetCommand(FontIconSymbols.ShareE72D);

    private string FollowText => IsFollowed ? IllustratorItemResources.Unfollow : IllustratorItemResources.Follow;

    public string GetIllustrationToolTipSubtitleText(User user)
    {
        return user.UserInfo.Comment ?? IllustratorItemResources.UserHasNoComment;
    }

    private async Task SetAvatarAsync()
    {
        var result = await App.AppViewModel.MakoClient.DownloadBitmapImageAsync(Illustrate.UserInfo.ProfileImageUrls.Medium, 100);
        AvatarSource = result is Result<ImageSource>.Success { Value: var avatar }
            ? avatar
            : await AppContext.GetPixivNoProfileImageAsync();
    }

    private async Task SetBackgroundAsync()
    {
        UserDetail = await App.AppViewModel.MakoClient.GetUserFromIdAsync(UserId, App.AppViewModel.AppSetting.TargetFilter);
        if (await App.AppViewModel.MakoClient.DownloadStreamAsync(UserDetail.UserProfile.BackgroundImageUrl) is Result<Stream>.Success(var stream))
        {
            //if (OverviewViewModel.AvatarBorderBrush is null)
            //{
            //    var dominantColor = await UiHelper.GetDominantColorAsync(stream.AsStreamForRead(), false);
            //    OverviewViewModel.AvatarBorderBrush = new SolidColorBrush(dominantColor);
            //}
            BackgroundSource = await stream.GetSoftwareBitmapSourceAsync(true);
        }
        else
            BackgroundSource = await AppContext.GetPixivNoProfileImageAsync();
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
        FollowCommand.ExecuteRequested += FollowCommandOnExecuteRequested;
        // TODO: ShareCommand
    }

    private void FollowCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (IsFollowed)
            Unfollow();
        else
            Follow();
        FollowCommand.Label = FollowText;
        FollowCommand.IconSource = MakoHelper.GetFollowButtonIcon(IsFollowed);
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

    public override void Dispose()
    {
        OverviewViewModel.Dispose();
    }
}
