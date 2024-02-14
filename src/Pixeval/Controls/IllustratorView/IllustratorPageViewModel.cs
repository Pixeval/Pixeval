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

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Pixeval.AppManagement;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Options;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util.IO;
using Pixeval.Utilities;

namespace Pixeval.Controls;

public partial class IllustratorPageViewModel : ObservableObject, IIllustrationVisualizer
{
    private const ThumbnailUrlOption Option = ThumbnailUrlOption.SquareMedium;

    [ObservableProperty]
    private ImageSource? _avatarSource;

    /// <summary>
    /// disable the follow button while follow request is being sent
    /// </summary>
    [ObservableProperty]
    private bool _isFollowButtonEnabled;

    [ObservableProperty]
    private bool _isFollowed;

    public IllustratorPageViewModel(UserInfo info)
    {
        Name = info.Name;
        AvatarUrl = info.ProfileImageUrls?.Medium!;
        Id = info.Id;
        Account = info.Account;
        Comment = info.Comment;
        IsFollowed = info.IsFollowed;
        Illustrations = [];
        IsFollowButtonEnabled = true;
        _ = LoadAvatar();
    }

    public string Name { get; set; }

    public string AvatarUrl { get; set; }

    public IFetchEngine<Illustration?> FetchEngine => App.AppViewModel.MakoClient.Posts(Id);

    public long Id { get; set; }

    public string? Account { get; set; }

    public string? Comment { get; set; }

    public ObservableCollection<IllustrationItemViewModel> Illustrations { get; set; }

    public void DisposeCurrent()
    {
        foreach (var illustration in Illustrations)
            illustration.UnloadThumbnail(this);
        Illustrations.Clear();
    }

    public void AddIllustrationViewModel(IllustrationItemViewModel viewModel)
    {
        Illustrations.Add(viewModel);
    }

    public async Task LoadAvatar()
    {
        if (AvatarSource is not null)
            return;
        var result = await App.AppViewModel.MakoClient.DownloadBitmapImageAsync(AvatarUrl, 60);
        AvatarSource = result is Result<ImageSource>.Success { Value: var avatar }
            ? avatar
            : await AppInfo.GetPixivNoProfileImageAsync();
    }

    public async Task Follow()
    {
        IsFollowButtonEnabled = false;
        await App.AppViewModel.MakoClient.PostFollowUserAsync(Id, PrivacyPolicy.Public);
        IsFollowed = true;
        IsFollowButtonEnabled = true;
    }

    public async Task PrivateFollow()
    {
        IsFollowButtonEnabled = false;
        await App.AppViewModel.MakoClient.PostFollowUserAsync(Id, PrivacyPolicy.Private);
        IsFollowed = true;
        IsFollowButtonEnabled = true;
    }

    public async Task Unfollow()
    {
        IsFollowButtonEnabled = false;
        await App.AppViewModel.MakoClient.RemoveFollowUserAsync(Id);
        IsFollowed = false;
        IsFollowButtonEnabled = true;
    }
}
