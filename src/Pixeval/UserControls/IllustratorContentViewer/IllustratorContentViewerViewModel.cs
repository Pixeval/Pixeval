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

using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Misc;
using Pixeval.UserControls.RecommendIllustratorProfile;
using Pixeval.Util.IO;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.UserControls.IllustratorContentViewer;

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
    
    public record UserMetrics(long FollowingCount, long MyPixivUsers /* 好P友 */, long IllustrationCount);

    [ObservableProperty]
    private NavigationViewTag? _illustrationTag;

    [ObservableProperty]
    private NavigationViewTag? _mangaTag;

    [ObservableProperty]
    private NavigationViewTag? _novelTag;

    [ObservableProperty]
    private NavigationViewTag? _bookmarkedIllustrationAndMangaTag;

    [ObservableProperty]
    private NavigationViewTag? _bookmarkedNovelTag;

    [ObservableProperty]
    private NavigationViewTag? _followingUserTag;

    [ObservableProperty]
    private NavigationViewTag? _myPixivUserTag;

    [ObservableProperty]
    private PixivSingleUserResponse _userDetail = null!;

    [ObservableProperty]
    private UserMetrics? _metrics;

    [ObservableProperty]
    private string? _username;

    [ObservableProperty]
    private bool _premium;

    [ObservableProperty]
    private bool _following;

    [ObservableProperty]
    private ImageSource? _avatar;

    [ObservableProperty]
    private XamlUICommand? _followCommand;

    [ObservableProperty]
    private XamlUICommand? _followPrivatelyCommand;

    [ObservableProperty]
    private ObservableCollection<MenuFlyoutItemBase> _actionMenuFlyoutItems = null!;

    [ObservableProperty]
    private IllustratorContentViewerTab _currentTab;

    public ObservableCollection<RecommendIllustratorProfileViewModel> RecommendIllustrators { get; init; }

    public IllustratorContentViewerViewModel(PixivSingleUserResponse userDetail)
    {
        RecommendIllustrators = new ObservableCollection<RecommendIllustratorProfileViewModel>();
        UserDetail = userDetail;
        
        InitializeTags();
        InitializeCommands();
        InitializeTriviaAsync().Discard();

        LoadRecommendIllustratorsAsync().Discard();
    }

    private void InitializeTags()
    {
        IllustrationTag = new NavigationViewTag(typeof(IllustratorIllustrationPage), UserDetail.UserEntity!.Id.ToString());
        MangaTag = new NavigationViewTag(typeof(IllustratorMangaPage), UserDetail.UserEntity!.Id.ToString());
    }

    private void InitializeCommands()
    {
        FollowCommand = GetFollowCommand();
        FollowCommand.ExecuteRequested += OnFollowCommandOnExecuteRequested;

        FollowPrivatelyCommand = new XamlUICommand
        {
            IconSource = FontIconSymbols.FavoriteStarE734.GetFontIconSource(),
            Label = IllustratorContentViewerResources.FollowPrivately
        };
        FollowPrivatelyCommand.CanExecuteRequested += (_, args) => args.CanExecute = !Following;
        FollowPrivatelyCommand.ExecuteRequested += FollowPrivatelyCommandOnExecuteRequested;
    }

    private async Task InitializeTriviaAsync()
    {
        var profile = UserDetail.UserProfile;

        CurrentTab = IllustratorContentViewerTab.Illustration;
        Username = UserDetail.UserEntity?.Name;
        Metrics = new UserMetrics(_userDetail.UserProfile?.TotalFollowUsers ?? 0, _userDetail.UserProfile?.TotalMyPixivUsers ?? 0, _userDetail.UserProfile?.TotalIllusts ?? 0);
        Premium = profile?.IsPremium ?? false;
        Following = UserDetail.UserEntity?.IsFollowed ?? false;
        Avatar = (await App.AppViewModel.MakoClient.DownloadBitmapImageResultAsync(UserDetail.UserEntity?.ProfileImageUrls?.Medium ?? string.Empty, 40)).GetOrElse(await AppContext.GetPixivNoProfileImageAsync());

        ActionMenuFlyoutItems = new ObservableCollection<MenuFlyoutItemBase>();
    }

    private void FollowPrivatelyCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (!Following)
        {
            Follow(true);
        }
    }

    private void OnFollowCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (Following)
        {
            Unfollow();
        }
        else
        {
            Follow(false);
        }

        FollowCommand = GetFollowCommand();
        FollowPrivatelyCommand?.NotifyCanExecuteChanged();
    }

    private XamlUICommand GetFollowCommand()
    {
        return new XamlUICommand
        {
            IconSource = Following ? FontIconSymbols.HeartFillEB52.GetFontIconSource(foregroundBrush: new SolidColorBrush(Colors.Crimson)) : FontIconSymbols.HeartEB51.GetFontIconSource(),
            Label = Following ? IllustratorContentViewerResources.Unfollow : IllustratorProfileResources.Follow
        };
    }

    private async Task LoadRecommendIllustratorsAsync()
    {
        static RecommendIllustratorProfileViewModel ToRecommendIllustratorProfileViewModel(PixivRelatedRecommendUsersResponse context, PixivRelatedRecommendUsersResponse.RecommendUser recommendUser)
        {
            var users = context.ResponseBody!.Users ?? Enumerates.ArrayOf<PixivRelatedRecommendUsersResponse.User>();
            var thumbnails = context.ResponseBody!.Thumbnails?.Illustrations ?? Enumerates.ArrayOf<PixivRelatedRecommendUsersResponse.Illust>();

            var userId = recommendUser.UserId;
            var user = users.First(u => u.UserId == userId);
            
            var displayImageUrls = (recommendUser.IllustIds ?? Enumerable.Empty<string>()).Select(id => thumbnails.First(t => t.Id == id)).SelectNotNull(illust => illust.Urls?.The250X250);

            return new RecommendIllustratorProfileViewModel(userId ?? string.Empty, user.Name ?? string.Empty, displayImageUrls, user.Image, user.Premium);
        }

        var recommendIllustrators = await App.AppViewModel.MakoClient.GetRelatedRecommendUsersAsync(UserDetail.UserEntity!.Id.ToString(), isR18: !App.AppViewModel.AppSetting.FiltrateRestrictedContent, lang: CultureInfo.CurrentUICulture);
        var viewModels = (recommendIllustrators.ResponseBody?.RecommendUsers ?? Enumerable.Empty<PixivRelatedRecommendUsersResponse.RecommendUser>())
            .Select(ru => ToRecommendIllustratorProfileViewModel(recommendIllustrators, ru));

        RecommendIllustrators.AddRange(viewModels);
    }

    private void Follow(bool privately)
    {
        Following = true;
        App.AppViewModel.MakoClient.PostFollowUserAsync(UserDetail.UserEntity!.Id.ToString(), privately ? PrivacyPolicy.Private : PrivacyPolicy.Public);
    }

    private void Unfollow()
    {
        Following = false;
        App.AppViewModel.MakoClient.RemoveFollowUserAsync(UserDetail.UserEntity!.Id.ToString());
    }
}