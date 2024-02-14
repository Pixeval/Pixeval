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
using Pixeval.CoreApi.Net.Response;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities;

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

    public RecommendIllustratorItemViewModel(RecommendUser user, IEnumerable<long>? ids) : base(user)
    {
        OverviewViewModel = new IllustratorIllustrationsOverviewViewModel(ids);

        InitializeCommands();
        FollowCommand.GetFollowCommand(IsFollowed);
    }

    public bool Premium => Illustrate.Premium;

    public string Username => Illustrate.Name;

    public long UserId => Illustrate.Id;

    public XamlUICommand FollowCommand { get; } = new();

    public IllustratorIllustrationsOverviewViewModel OverviewViewModel { get; }

    private void InitializeCommands()
    {
        FollowCommand.ExecuteRequested += FollowCommandOnExecuteRequested;
    }

    public async Task LoadAvatarAsync()
    {
        var result = await App.AppViewModel.MakoClient.DownloadBitmapImageAsync(Illustrate.Image, 100);
        AvatarSource = result is Result<ImageSource>.Success { Value: var avatar }
            ? avatar
            : await AppInfo.GetPixivNoProfileImageAsync();
        await OverviewViewModel.LoadBannerSource();
    }

    public string GetIllustrationToolTipSubtitleText(RecommendUser user)
    {
        return user.Comment is "" ? IllustrateItemResources.UserHasNoComment : user.Comment;
    }

    private void FollowCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        IsFollowed = MakoHelper.SetFollow(UserId, !IsFollowed);
        ButtonStyle = Application.Current.Resources[IsFollowed ? "DefaultButtonStyle" : "AccentButtonStyle"].To<Style>();
        FollowCommand.GetFollowCommand(IsFollowed);
    }

    public override void Dispose()
    {
        OverviewViewModel.Dispose();
    }
}
