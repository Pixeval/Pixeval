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

using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.AppManagement;
using Pixeval.Controls.Illustrate;
using Pixeval.CoreApi.Model;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;

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
        IsFollowed = Illustrate.UserInfo.IsFollowed;

        InitializeCommands();
        FollowCommand.GetFollowCommand(IsFollowed);
    }

    public IllustratorIllustrationsOverviewViewModel OverviewViewModel { get; }

    public string Username => Illustrate.UserInfo.Name;

    public long UserId => Illustrate.UserInfo.Id;

    public async Task LoadAvatarAsync()
    {
        var result = await App.AppViewModel.MakoClient.DownloadBitmapImageAsync(Illustrate.UserInfo.ProfileImageUrls.Medium, 100);
        AvatarSource = result is Result<ImageSource>.Success { Value: var avatar }
            ? avatar
            : await AppInfo.GetPixivNoProfileImageAsync();
        await OverviewViewModel.LoadBannerSource();
    }

    public override void Dispose()
    {
        OverviewViewModel.Dispose();
    }
}
