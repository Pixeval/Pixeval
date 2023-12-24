#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/RecommendIllustratorProfileViewModel.cs
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
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pixeval.AppManagement;
using Pixeval.Controls.Illustrate;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Util.IO;
using WinUI3Utilities;
using RecommendUser = Pixeval.CoreApi.Net.Response.PixivRelatedRecommendUsersResponse.User;

namespace Pixeval.Controls;

public partial class RecommendIllustratorItemViewModel : IllustrateViewModel<RecommendUser>
{
    [ObservableProperty]
    private ImageSource? _avatarSource;

    [ObservableProperty]
    private Style _buttonStyle = Application.Current.Resources["AccentButtonStyle"].To<Style>();

    // this value does not have to be initialized to `true` or `false` because the recommend illustrators are guaranteed to be not followed yet.
    [ObservableProperty]
    private bool _isFollowed;

    public RecommendIllustratorItemViewModel(RecommendUser user, IEnumerable<string>? ids) : base(user)
    {
        OverviewViewModel = new IllustratorIllustrationsOverviewViewModel(ids);

        _ = SetAvatarAsync();

        FollowCommand = new XamlUICommand { Label = FollowText };
        InitializeCommands();
    }

    public Visibility Premium => Illustrate.Premium ? Visibility.Visible : Visibility.Collapsed;

    public string Username => Illustrate.Name ?? "";

    public string UserId => Illustrate.Id ?? "";

    public XamlUICommand FollowCommand { get; set; }

    public IllustratorIllustrationsOverviewViewModel OverviewViewModel { get; }

    private string FollowText => IsFollowed ? RecommendIllustratorItemResources.FollowButtonUnfollow : RecommendIllustratorItemResources.FollowButtonFollow;

    private void InitializeCommands()
    {
        FollowCommand.ExecuteRequested += FollowCommandOnExecuteRequested;
    }

    private async Task SetAvatarAsync()
    {
        if (Illustrate.Image is { } url)
        {
            var avatar = await App.AppViewModel.MakoClient.DownloadBitmapImageResultAsync(url, 100);
            AvatarSource = avatar.UnwrapOrElse(await AppContext.GetPixivNoProfileImageAsync());
        }
        else
            AvatarSource = await AppContext.GetPixivNoProfileImageAsync();
    }

    public string GetIllustrationToolTipSubtitleText(RecommendUser? user)
    {
        return user?.Comment ?? RecommendIllustratorItemResources.UserHasNoComment;
    }

    private void FollowCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (IsFollowed)
            Unfollow();
        else
            Follow();
        FollowCommand.Label = FollowText;
    }

    private void Follow()
    {
        IsFollowed = true;
        _ = App.AppViewModel.MakoClient.PostFollowUserAsync(UserId, PrivacyPolicy.Public);
        ButtonStyle = Application.Current.Resources["DefaultButtonStyle"].To<Style>();
    }

    private void Unfollow()
    {
        IsFollowed = false;
        _ = App.AppViewModel.MakoClient.RemoveFollowUserAsync(UserId);
        ButtonStyle = Application.Current.Resources["AccentButtonStyle"].To<Style>();
    }

    public override void Dispose()
    {
        OverviewViewModel.Dispose();
    }
}
