#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustratorContentViewerViewModel.cs
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
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pixeval.AppManagement;
using Pixeval.Controls.MarkupExtensions;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;

namespace Pixeval.Controls.IllustratorContentViewer;

public partial class IllustratorContentViewerViewModel : ObservableObject
{
    public enum IllustratorContentViewerTab
    {
        Illustration,
        Manga,
        Novel,
        BookmarkedIllustrationAndManga,
        BookmarkedNovel,
        FollowingUser,
        MyPixivUser
    }

    [ObservableProperty]
    private ImageSource? _avatarSource;

    [ObservableProperty]
    private IllustratorContentViewerTab _currentTab = IllustratorContentViewerTab.Illustration;

    private bool _isFollowed;

    public bool IsFollowed
    {
        get => _isFollowed;
        set
        {
            if (_isFollowed == value)
                return;
            _isFollowed = value;
            OnPropertyChanged();
            FollowPrivatelyCommand.NotifyCanExecuteChanged();
            FollowCommand.GetFollowCommand(IsFollowed);
        }
    }

    public NavigationViewTag IllustrationTag { get; }

    public NavigationViewTag MangaTag { get; }

    public NavigationViewTag BookmarkedIllustrationAndMangaTag { get; }

    public NavigationViewTag FollowingUserTag { get; }

    public NavigationViewTag MyPixivUserTag { get; }

    public NavigationViewTag NovelTag { get; }

    public NavigationViewTag BookmarkedNovelTag { get; }

    public bool Premium => UserDetail.UserProfile.IsPremium;

    public string UserName => UserDetail.UserEntity.Name;

    public long UserId => UserDetail.UserEntity.Id;

    public PixivSingleUserResponse UserDetail { get; }

    public UserMetrics Metrics { get; }

    public XamlUICommand FollowCommand { get; } = "".GetCommand(FontIconSymbols.ContactE77B);

    public XamlUICommand FollowPrivatelyCommand { get; } = IllustratorContentViewerResources.FollowPrivately.GetCommand(FontIconSymbols.FavoriteStarE734);

    public IllustratorContentViewerViewModel(PixivSingleUserResponse userDetail)
    {
        RecommendIllustrators = [];
        UserDetail = userDetail;
        Metrics = new UserMetrics(userDetail.UserProfile.TotalFollowUsers, userDetail.UserProfile.TotalMyPixivUsers, userDetail.UserProfile.TotalIllusts);

        IllustrationTag = new NavigationViewTag(typeof(IllustratorIllustrationPage), UserDetail.UserEntity.Id);
        MangaTag = new NavigationViewTag(typeof(IllustratorMangaPage), UserDetail.UserEntity.Id);
        BookmarkedIllustrationAndMangaTag = new NavigationViewTag(typeof(IllustratorIllustrationAndMangaBookmarkPage), UserDetail.UserEntity.Id);
        FollowingUserTag = null!;
        MyPixivUserTag = null!;
        NovelTag = null!;
        BookmarkedNovelTag = null!;

        InitializeCommands();
        // 在InitializeCommands之后，方便setter里的触发
        IsFollowed = UserDetail.UserEntity.IsFollowed;
        _ = SetAvatarAsync();
    }

    public bool ShowRecommendIllustrators
    {
        get => App.AppViewModel.AppSettings.ShowRecommendIllustratorsInIllustratorContentViewer;
        set => SetProperty(App.AppViewModel.AppSettings.ShowRecommendIllustratorsInIllustratorContentViewer, value, App.AppViewModel.AppSettings, (setting, value) =>
        {
            setting.ShowRecommendIllustratorsInIllustratorContentViewer = value;
            AppInfo.SaveConfig(App.AppViewModel.AppSettings);
            ShowRecommendIllustratorsChanged?.Invoke(this, value);
        });
    }

    public bool ShowExternalCommandBar
    {
        get => App.AppViewModel.AppSettings.ShowExternalCommandBarInIllustratorContentViewer;
        set => SetProperty(App.AppViewModel.AppSettings.ShowExternalCommandBarInIllustratorContentViewer, value, App.AppViewModel.AppSettings, (setting, value) =>
        {
            setting.ShowExternalCommandBarInIllustratorContentViewer = value; 
            AppInfo.SaveConfig(App.AppViewModel.AppSettings);
            ShowExternalCommandBarChanged?.Invoke(this, value);
        });
    }

    public ObservableCollection<RecommendIllustratorItemViewModel> RecommendIllustrators { get; }

    public event EventHandler<bool>? ShowRecommendIllustratorsChanged;

    public event EventHandler<bool>? ShowExternalCommandBarChanged;

    private async Task SetAvatarAsync()
    {
        var result = await App.AppViewModel.MakoClient.DownloadBitmapImageAsync(UserDetail.UserEntity.ProfileImageUrls.Medium, 40);
        AvatarSource = result is Result<ImageSource>.Success { Value: var avatar }
            ? avatar
            : await AppInfo.GetPixivNoProfileImageAsync();
    }

    private void InitializeCommands()
    {
        FollowCommand.ExecuteRequested += (_, _) => IsFollowed = MakoHelper.SetFollow(UserId, !IsFollowed);

        FollowPrivatelyCommand.CanExecuteRequested += (_, args) => args.CanExecute = !IsFollowed;
        FollowPrivatelyCommand.ExecuteRequested += FollowPrivatelyCommandOnExecuteRequested;
    }

    private void FollowPrivatelyCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (!IsFollowed)
            IsFollowed = MakoHelper.SetFollow(UserId, true, true);
    }

    public async Task LoadRecommendIllustratorsAsync()
    {
        var recommendIllustrators = await App.AppViewModel.MakoClient.GetRelatedRecommendUsersAsync(UserDetail.UserEntity.Id, isR18: !App.AppViewModel.AppSettings.FiltrateRestrictedContent, lang: CultureInfo.CurrentUICulture);
        var viewModels = recommendIllustrators.ResponseBody.RecommendMaps
            .Select(ru => ToRecommendIllustratorProfileViewModel(recommendIllustrators, ru));

        RecommendIllustrators.AddRange(viewModels);
        return;

        static RecommendIllustratorItemViewModel ToRecommendIllustratorProfileViewModel(PixivRelatedRecommendUsersResponse context, RecommendMap recommendUser)
        {
            var users = context.ResponseBody.Users;
            var thumbnails = context.ResponseBody.Thumbnails.Illustrations;

            var userId = recommendUser.UserId;
            var user = users.First(u => u.Id == userId);

            return new RecommendIllustratorItemViewModel(user, recommendUser.IllustIds);
        }
    }

    public Visibility GetNavigationViewAutoSuggestBoxVisibility(bool showExternalCommandBar)
    {
        return (!showExternalCommandBar || CurrentTab is not (IllustratorContentViewerTab.Illustration or IllustratorContentViewerTab.Manga or IllustratorContentViewerTab.BookmarkedIllustrationAndManga)).ToVisibility();
    }

    public record UserMetrics(long FollowingCount, long MyPixivUsers /* 好P友 */, long IllustrationCount);
}
