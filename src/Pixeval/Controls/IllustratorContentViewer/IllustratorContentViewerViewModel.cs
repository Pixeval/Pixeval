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
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pixeval.Controls.IllustrationView;
using Pixeval.Controls.MarkupExtensions;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Misc;
using Pixeval.Util.IO;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

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
    private ObservableCollection<MenuFlyoutItemBase> _actionMenuFlyoutItems = null!;

    [ObservableProperty]
    private ImageSource? _avatar;

    [ObservableProperty]
    private NavigationViewTag? _bookmarkedIllustrationAndMangaTag;

    [ObservableProperty]
    private NavigationViewTag? _bookmarkedNovelTag;

    [ObservableProperty]
    private IllustratorContentViewerTab _currentTab;

    [ObservableProperty]
    private XamlUICommand? _followCommand;

    [ObservableProperty]
    private bool _following;

    [ObservableProperty]
    private NavigationViewTag? _followingUserTag;

    [ObservableProperty]
    private XamlUICommand? _followPrivatelyCommand;

    [ObservableProperty]
    private NavigationViewTag? _illustrationTag;

    [ObservableProperty]
    private NavigationViewTag? _mangaTag;

    [ObservableProperty]
    private UserMetrics? _metrics;

    [ObservableProperty]
    private NavigationViewTag? _myPixivUserTag;

    [ObservableProperty]
    private NavigationViewTag? _novelTag;

    [ObservableProperty]
    private bool _premium;

    [ObservableProperty]
    private PixivSingleUserResponse _userDetail = null!;

    [ObservableProperty]
    private string? _username;

    public IllustratorContentViewerViewModel(PixivSingleUserResponse userDetail)
    {
        RecommendIllustrators = [];
        UserDetail = userDetail;

        InitializeTags();
        InitializeTrivia();
        InitializeCommands();
        SetAvatarAsync().Discard();
    }

    public bool ShowRecommendIllustrators
    {
        get => App.AppViewModel.AppSetting.ShowRecommendIllustratorsInIllustratorContentViewer;
        set => SetProperty(App.AppViewModel.AppSetting.ShowRecommendIllustratorsInIllustratorContentViewer, value, App.AppViewModel.AppSetting, (setting, value) =>
        {
            setting.ShowRecommendIllustratorsInIllustratorContentViewer = value;
            ShowRecommendIllustratorsChanged?.Invoke(this, value);
        });
    }

    public bool ShowExternalCommandBar
    {
        get => App.AppViewModel.AppSetting.ShowExternalCommandBarInIllustratorContentViewer;
        set => SetProperty(App.AppViewModel.AppSetting.ShowExternalCommandBarInIllustratorContentViewer, value, App.AppViewModel.AppSetting, (setting, value) =>
        {
            setting.ShowExternalCommandBarInIllustratorContentViewer = value;
            ShowExternalCommandBarChanged?.Invoke(this, value);
        });
    }

    public ObservableCollection<RecommendIllustratorItemViewModel> RecommendIllustrators { get; init; }

    public event EventHandler<bool>? ShowRecommendIllustratorsChanged;

    public event EventHandler<bool>? ShowExternalCommandBarChanged;

    private void InitializeTags()
    {
        IllustrationTag = new NavigationViewTag(typeof(IllustratorIllustrationPage), UserDetail.UserEntity!.Id.ToString());
        MangaTag = new NavigationViewTag(typeof(IllustratorMangaPage), UserDetail.UserEntity!.Id.ToString());
        BookmarkedIllustrationAndMangaTag = new NavigationViewTag(typeof(IllustratorIllustrationAndMangaBookmarkPage), UserDetail.UserEntity!.Id.ToString());
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

    private void InitializeTrivia()
    {
        var profile = UserDetail.UserProfile;

        CurrentTab = IllustratorContentViewerTab.Illustration;
        Username = UserDetail.UserEntity?.Name;
        Metrics = new UserMetrics(profile?.TotalFollowUsers ?? 0, profile?.TotalMyPixivUsers ?? 0, profile?.TotalIllusts ?? 0);
        Premium = profile?.IsPremium ?? false;
        Following = UserDetail.UserEntity?.IsFollowed ?? false;
        ActionMenuFlyoutItems = [];
    }

    private async Task SetAvatarAsync()
    {
        Avatar = (await App.AppViewModel.MakoClient.DownloadBitmapImageResultAsync(UserDetail.UserEntity?.ProfileImageUrls?.Medium ?? string.Empty, 40)).UnwrapOrElse(await AppContext.GetPixivNoProfileImageAsync());
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
            Label = Following ? IllustratorContentViewerResources.Unfollow : IllustratorItemResources.Follow
        };
    }

    public async Task LoadRecommendIllustratorsAsync()
    {
        // TODO
        try
        {
            var recommendIllustrators = await App.AppViewModel.MakoClient.GetRelatedRecommendUsersAsync(UserDetail.UserEntity!.Id.ToString(), isR18: !App.AppViewModel.AppSetting.FiltrateRestrictedContent, lang: CultureInfo.CurrentUICulture);
            var viewModels = (recommendIllustrators.ResponseBody?.RecommendUsers ?? [])
                .Select(ru => ToRecommendIllustratorProfileViewModel(recommendIllustrators, ru));

            RecommendIllustrators.AddRange(viewModels);
            return;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        static RecommendIllustratorItemViewModel ToRecommendIllustratorProfileViewModel(PixivRelatedRecommendUsersResponse context, PixivRelatedRecommendUsersResponse.RecommendUser recommendUser)
        {
            var users = context.ResponseBody!.Users ?? [];
            var thumbnails = context.ResponseBody!.Thumbnails?.Illustrations ?? [];

            var userId = recommendUser.UserId;
            var user = users.First(u => u.Id == userId);
            
            return new RecommendIllustratorItemViewModel(user, recommendUser.IllustIds);
        }
    }

    private void Follow(bool privately)
    {
        Following = true;
        _ = App.AppViewModel.MakoClient.PostFollowUserAsync(UserDetail.UserEntity!.Id.ToString(), privately ? PrivacyPolicy.Private : PrivacyPolicy.Public);
    }

    private void Unfollow()
    {
        Following = false;
        _ = App.AppViewModel.MakoClient.RemoveFollowUserAsync(UserDetail.UserEntity!.Id.ToString());
    }

    public Visibility GetNavigationViewAutoSuggestBoxVisibility(bool showExternalCommandBar)
    {
        return (!showExternalCommandBar || CurrentTab is not (IllustratorContentViewerTab.Illustration or IllustratorContentViewerTab.Manga or IllustratorContentViewerTab.BookmarkedIllustrationAndManga)).ToVisibility();
    }

    public record UserMetrics(long FollowingCount, long MyPixivUsers /* 好P友 */, long IllustrationCount);
}
