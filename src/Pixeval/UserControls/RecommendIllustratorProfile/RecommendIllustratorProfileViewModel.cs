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
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.AppManagement;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

namespace Pixeval.UserControls.RecommendIllustratorProfile;

public partial class RecommendIllustratorProfileViewModel : ObservableObject
{
    private readonly string _userId;

    // this value does not have to be initialized to `true` or `false` because the recommend illustrators are guaranteed to be not followed yet.
    [ObservableProperty]
    private bool _followed;

    [ObservableProperty]
    private bool _premium;

    [ObservableProperty]
    private string? _username;
    
    public Task<SoftwareBitmapSource[]>? DisplayImagesTask { get; set; }

    public Task<ImageSource>? AvatarTask { get; set; }

    public RecommendIllustratorProfileViewModel(string userId, string? username, IEnumerable<string> displayImagesUrl, string? avatarUrl, bool premium)
    {
        _userId = userId;
        _username = username;
        Premium = premium;
        DisplayImagesTask = GetDisplayImageTaskAsync(displayImagesUrl);
        AvatarTask = GetAvatarTaskAsync(avatarUrl);
    }

    private static async Task<SoftwareBitmapSource[]> GetDisplayImageTaskAsync(IEnumerable<string> displayImages)
    {
        var results = await Task.WhenAll(displayImages.Select(App.AppViewModel.MakoClient.DownloadSoftwareBitmapSourceResultAsync));
        return results.SelectNotNull(r => r is Result<ImageSource>.Success(SoftwareBitmapSource s) ? s : null).ToArray();
    }

    private static async Task<ImageSource> GetAvatarTaskAsync(string? avatarUrl)
    {
        if (avatarUrl.IsNullOrBlank())
        {
            return await AppContext.GetPixivNoProfileImageAsync();
        }
             
        return await App.AppViewModel.MakoClient.DownloadBitmapImageResultAsync(avatarUrl!, 45) switch
        {
            Result<ImageSource>.Success(var s) => s,
            Result<ImageSource>.Failure => await AppContext.GetPixivNoProfileImageAsync(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void Follow()
    {
        Followed = true;
        App.AppViewModel.MakoClient.PostFollowUserAsync(_userId, PrivacyPolicy.Public);
    }

    public void Unfollow()
    {
        Followed = false;
        App.AppViewModel.MakoClient.RemoveFollowUserAsync(_userId);
    }

    public SolidColorBrush GetButtonBackground(bool isFollowed)
    {
        return isFollowed ? new SolidColorBrush(UIHelper.ParseHexColor("#525252")) : new SolidColorBrush(UIHelper.ParseHexColor("#0096FA"));
    }

    public SolidColorBrush GetButtonForeground(bool isFollowed)
    {
        return isFollowed ? new SolidColorBrush(UIHelper.ParseHexColor("#DBDBDB")) : new SolidColorBrush(UIHelper.ParseHexColor("#F5F5F5"));
    }

    public string GetFollowButtonText(bool isFollowed)
    {
        return isFollowed ? RecommendIllustratorProfileResources.FollowButtonUnfollow : RecommendIllustratorProfileResources.FollowButtonFollow;
    }
}