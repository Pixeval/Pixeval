// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Mako.Engine;
using Mako.Engine.Implements;
using Mako.Global.Enum;
using Mako.Model;
using Misaki;
using Pixeval.Controls;
using Pixeval.Models.Home;
using Pixeval.Models.Options;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.ViewModels.Home;
using Pixeval.Views.Capability;
using Pixeval.Views.Search;
using Pixeval.Views.Viewers;

namespace Pixeval.Views.Home;

public static class HomeCardDefinitions
{
    private static readonly FrozenDictionary<HomePageCardSourceKind, HomeCardDefinition> _BySourceKind;

    static HomeCardDefinitions()
    {
        All =
        [
            new(
                HomePageCardSourceKind.WorkRecommended,
                HomePageCardTemplateKind.WorkList,
                HomeCardParameterKinds.WorkType,
                CreateWorkViewModelFactory(
                    card => App.AppViewModel.MakoClient.WorkRecommended(card.WorkType),
                    _ => CreateNovelEngine(App.AppViewModel.MakoClient.WorkRecommended(WorkType.Novel)),
                    static card => card.WorkType is WorkType.Novel),
                OpenWorkRecommendedPage,
                card => [GetDescription(card.WorkType)]),
            new(
                HomePageCardSourceKind.WorkBookmarks,
                HomePageCardTemplateKind.WorkList,
                HomeCardParameterKinds.UserId
                | HomeCardParameterKinds.SimpleWorkType
                | HomeCardParameterKinds.PrivacyPolicy
                | HomeCardParameterKinds.Tag,
                CreateWorkViewModelFactory(
                    card => App.AppViewModel.MakoClient.WorkBookmarks(card.UserId, card.SimpleWorkType, card.PrivacyPolicy, card.Tag),
                    card => CreateNovelEngine(App.AppViewModel.MakoClient.WorkBookmarks(card.UserId, SimpleWorkType.Novel, card.PrivacyPolicy, card.Tag)),
                    static card => card.SimpleWorkType is SimpleWorkType.Novel),
                OpenWorkBookmarksPage,
                card =>
                [
                    $"@{card.UserId}",
                    GetDescription(card.SimpleWorkType),
                    GetDescription(card.PrivacyPolicy),
                    .. string.IsNullOrWhiteSpace(card.Tag) ? [] : new[] { $"#{card.Tag}" }
                ],
                useCurrentUserAsDefault: true),
            new(
                HomePageCardSourceKind.WorkRanking,
                HomePageCardTemplateKind.WorkList,
                HomeCardParameterKinds.SimpleWorkType
                | HomeCardParameterKinds.RankOption
                | HomeCardParameterKinds.RankingDate,
                CreateWorkViewModelFactory(
                    card => App.AppViewModel.MakoClient.WorkRanking(card.SimpleWorkType, card.RankOption, card.GetRankingDate()),
                    card => CreateNovelEngine(App.AppViewModel.MakoClient.WorkRanking(SimpleWorkType.Novel, card.RankOption, card.GetRankingDate())),
                    static card => card.SimpleWorkType is SimpleWorkType.Novel),
                OpenWorkRankingPage,
                card =>
                [
                    GetDescription(card.SimpleWorkType),
                    GetRankOptionDescription(card),
                    .. card.UseSpecifiedRankingDate
                        ? new[] { card.GetRankingDate().LocalDateTime.ToString("d", CultureInfo.CurrentCulture) }
                        : []
                ]),
            new(
                HomePageCardSourceKind.WorkNew,
                HomePageCardTemplateKind.WorkList,
                HomeCardParameterKinds.WorkType,
                CreateWorkViewModelFactory(
                    card => App.AppViewModel.MakoClient.WorkNew(card.WorkType),
                    _ => CreateNovelEngine(App.AppViewModel.MakoClient.WorkNew(WorkType.Novel)),
                    static card => card.WorkType is WorkType.Novel),
                OpenWorkNewPage,
                card => [GetDescription(card.WorkType)]),
            new(
                HomePageCardSourceKind.WorkFollowing,
                HomePageCardTemplateKind.WorkList,
                HomeCardParameterKinds.SimpleWorkType | HomeCardParameterKinds.PrivacyPolicy,
                CreateWorkViewModelFactory(
                    card => App.AppViewModel.MakoClient.WorkFollowing(card.SimpleWorkType, card.PrivacyPolicy),
                    card => CreateNovelEngine(App.AppViewModel.MakoClient.WorkFollowing(SimpleWorkType.Novel, card.PrivacyPolicy)),
                    static card => card.SimpleWorkType is SimpleWorkType.Novel),
                OpenWorkFollowingPage,
                card => [GetDescription(card.SimpleWorkType), GetDescription(card.PrivacyPolicy)]),
            new(
                HomePageCardSourceKind.WorkMyPixiv,
                HomePageCardTemplateKind.WorkList,
                HomeCardParameterKinds.SimpleWorkType,
                CreateWorkViewModelFactory(
                    card => App.AppViewModel.MakoClient.WorkMyPixiv(card.SimpleWorkType),
                    _ => CreateNovelEngine(App.AppViewModel.MakoClient.WorkMyPixiv(SimpleWorkType.Novel)),
                    static card => card.SimpleWorkType is SimpleWorkType.Novel),
                OpenWorkMyPixivPage,
                card => [GetDescription(card.SimpleWorkType)]),
            new(
                HomePageCardSourceKind.WorkRelated,
                HomePageCardTemplateKind.WorkList,
                HomeCardParameterKinds.EntryId | HomeCardParameterKinds.SimpleWorkType,
                CreateWorkViewModelFactory(
                    card => App.AppViewModel.MakoClient.WorkRelated(card.EntryId, card.SimpleWorkType),
                    card => CreateNovelEngine(App.AppViewModel.MakoClient.WorkRelated(card.EntryId, SimpleWorkType.Novel)),
                    static card => card.SimpleWorkType is SimpleWorkType.Novel),
                OpenWorkRelatedPage,
                card => [card.EntryId.ToString(CultureInfo.InvariantCulture), GetDescription(card.SimpleWorkType)]),
            new(
                HomePageCardSourceKind.WorkPosts,
                HomePageCardTemplateKind.WorkList,
                HomeCardParameterKinds.UserId | HomeCardParameterKinds.WorkType,
                CreateWorkViewModelFactory(
                    card => App.AppViewModel.MakoClient.WorkPosted(card.UserId, card.WorkType),
                    card => CreateNovelEngine(App.AppViewModel.MakoClient.WorkPosted(card.UserId, WorkType.Novel)),
                    static card => card.WorkType is WorkType.Novel),
                OpenWorkPostsPage,
                card => [$"@{card.UserId}", GetDescription(card.WorkType)],
                useCurrentUserAsDefault: true),
            new(
                HomePageCardSourceKind.WorkSearch,
                HomePageCardTemplateKind.WorkList,
                HomeCardParameterKinds.SimpleWorkType | HomeCardParameterKinds.SearchText,
                CreateWorkViewModelFactory(
                    card => string.IsNullOrWhiteSpace(card.SearchText)
                        ? App.AppViewModel.MakoClient.Computed(AsyncEnumerable.Empty<IArtworkInfo>())
                        : App.AppViewModel.MakoClient.IllustrationSearch(new IllustrationSearchArguments(card.SearchText)),
                    card => string.IsNullOrWhiteSpace(card.SearchText)
                        ? App.AppViewModel.MakoClient.Computed(AsyncEnumerable.Empty<Novel>())
                        : App.AppViewModel.MakoClient.NovelSearch(new NovelSearchArguments(card.SearchText)),
                    static card => card.SimpleWorkType is SimpleWorkType.Novel),
                OpenWorkSearchPage,
                card => [GetDescription(card.SimpleWorkType), card.SearchText ?? ""]),
            new(
                HomePageCardSourceKind.UserRecommended,
                HomePageCardTemplateKind.UserList,
                HomeCardParameterKinds.None,
                CreateUserViewModelFactory(_ => App.AppViewModel.MakoClient.UserRecommended()),
                OpenUserRecommendedPage),
            new(
                HomePageCardSourceKind.UserSearch,
                HomePageCardTemplateKind.UserList,
                HomeCardParameterKinds.SearchText,
                CreateUserViewModelFactory(card => string.IsNullOrWhiteSpace(card.SearchText)
                    ? App.AppViewModel.MakoClient.Computed(AsyncEnumerable.Empty<User>())
                    : App.AppViewModel.MakoClient.UserSearch(card.SearchText)),
                OpenUserSearchPage,
                card => [card.SearchText ?? ""]),
            new(
                HomePageCardSourceKind.UserFollowing,
                HomePageCardTemplateKind.UserList,
                HomeCardParameterKinds.UserId | HomeCardParameterKinds.PrivacyPolicy,
                CreateUserViewModelFactory(card => App.AppViewModel.MakoClient.UserFollowing(card.UserId, card.PrivacyPolicy)),
                OpenUserFollowingPage,
                card => [$"@{card.UserId}", GetDescription(card.PrivacyPolicy)],
                useCurrentUserAsDefault: true),
            new(
                HomePageCardSourceKind.UserFollower,
                HomePageCardTemplateKind.UserList,
                HomeCardParameterKinds.None,
                CreateUserViewModelFactory(_ => App.AppViewModel.MakoClient.UserFollower()),
                OpenUserFollowerPage),
            new(
                HomePageCardSourceKind.UserMyPixiv,
                HomePageCardTemplateKind.UserList,
                HomeCardParameterKinds.UserId,
                CreateUserViewModelFactory(card => App.AppViewModel.MakoClient.UserMyPixiv(card.UserId)),
                OpenUserMyPixivPage,
                card => [$"@{card.UserId}"],
                useCurrentUserAsDefault: true),
            new(
                HomePageCardSourceKind.Spotlight,
                HomePageCardTemplateKind.SpotlightList,
                HomeCardParameterKinds.None,
                CreateSpotlightViewModelAsync,
                OpenSpotlightPage),
            new(
                HomePageCardSourceKind.SingleImage,
                HomePageCardTemplateKind.SingleImage,
                HomeCardParameterKinds.EntryId,
                CreateSingleImageViewModelAsync,
                OpenFirstPreviewItem,
                card => [card.EntryId.ToString(CultureInfo.InvariantCulture)]),
            new(
                HomePageCardSourceKind.SingleNovel,
                HomePageCardTemplateKind.SingleNovel,
                HomeCardParameterKinds.EntryId,
                CreateSingleNovelViewModelAsync,
                OpenFirstPreviewItem,
                card => [card.EntryId.ToString(CultureInfo.InvariantCulture)],
                workType: WorkType.Novel,
                simpleWorkType: SimpleWorkType.Novel),
            new(
                HomePageCardSourceKind.SingleUser,
                HomePageCardTemplateKind.SingleUser,
                HomeCardParameterKinds.UserId,
                CreateSingleUserViewModelAsync,
                OpenFirstPreviewItem,
                card => [$"@{card.UserId}"])
        ];
        _BySourceKind = All.ToFrozenDictionary(static definition => definition.SourceKind);
    }

    public static IReadOnlyList<HomeCardDefinition> All { get; }

    public static HomeCardDefinition Get(HomePageCardSourceKind sourceKind) =>
        _BySourceKind.TryGetValue(sourceKind, out var definition)
            ? definition
            : _BySourceKind[HomePageCardSourceKind.WorkRecommended];

    public static void OpenPreviewItem(HomeCardPreviewViewModel? previewViewModel, TopLevel topLevel, object? parameter)
    {
        switch (parameter)
        {
            case NovelItemViewModel viewModel:
                OpenNovel(topLevel, previewViewModel?.ViewModel, viewModel);
                break;
            case IllustrationItemViewModel viewModel:
                OpenIllustration(topLevel, previewViewModel?.ViewModel, viewModel);
                break;
            case UserItemViewModel viewModel:
                topLevel.ViewContainer?.CreateUserPage(viewModel.UserId);
                break;
            case SpotlightItemViewModel viewModel:
                if (topLevel.Launcher is { } launcher)
                    _ = launcher.LaunchUriAsync(new(viewModel.Entry.ArticleUrl));
                break;
        }
    }

    private static Func<HomePageCardLayout, Task<ISimpleViewViewModel>> CreateWorkViewModelFactory(
        Func<HomePageCardLayout, IFetchEngine<IArtworkInfo>> illustrationEngineFactory,
        Func<HomePageCardLayout, IFetchEngine<Novel>> novelEngineFactory,
        Predicate<HomePageCardLayout> useNovelEngine) =>
        card => Task.FromResult<ISimpleViewViewModel>(
            useNovelEngine(card)
                ? CreateNovelViewModel(novelEngineFactory(card))
                : CreateIllustrationViewModel(illustrationEngineFactory(card)));

    private static Func<HomePageCardLayout, Task<ISimpleViewViewModel>> CreateUserViewModelFactory(
        Func<HomePageCardLayout, IFetchEngine<User>> engineFactory) =>
        card => Task.FromResult<ISimpleViewViewModel>(CreateUserViewModel(engineFactory(card)));

    private static Task<ISimpleViewViewModel> CreateSpotlightViewModelAsync(HomePageCardLayout card) =>
        Task.FromResult<ISimpleViewViewModel>(CreateSpotlightViewModel(App.AppViewModel.MakoClient.Spotlight()));

    private static async Task<ISimpleViewViewModel> CreateSingleImageViewModelAsync(HomePageCardLayout card) =>
        CreateIllustrationViewModel(App.AppViewModel.MakoClient.Computed(Single(await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(card.EntryId))));

    private static async Task<ISimpleViewViewModel> CreateSingleNovelViewModelAsync(HomePageCardLayout card) =>
        CreateNovelViewModel(App.AppViewModel.MakoClient.Computed(Single(await App.AppViewModel.MakoClient.GetNovelFromIdAsync(card.EntryId))));

    private static async Task<ISimpleViewViewModel> CreateSingleUserViewModelAsync(HomePageCardLayout card) =>
        CreateUserViewModel(App.AppViewModel.MakoClient.Computed(Single(CreateUser((await App.AppViewModel.MakoClient.GetUserFromIdAsync(card.UserId)).UserEntity))));

    private static IllustrationViewViewModel CreateIllustrationViewModel(IFetchEngine<IArtworkInfo> engine)
    {
        var viewModel = new IllustrationViewViewModel();
        viewModel.ResetEngine(engine);
        return viewModel;
    }

    private static NovelViewViewModel CreateNovelViewModel(IFetchEngine<Novel> engine)
    {
        var viewModel = new NovelViewViewModel();
        viewModel.ResetEngine(engine);
        return viewModel;
    }

    private static UserViewViewModel CreateUserViewModel(IFetchEngine<User> engine)
    {
        var viewModel = new UserViewViewModel();
        viewModel.ResetEngine(engine, static (user, _) => new(user));
        return viewModel;
    }

    private static SpotlightViewViewModel CreateSpotlightViewModel(IFetchEngine<Spotlight> engine)
    {
        var viewModel = new SpotlightViewViewModel();
        viewModel.ResetEngine(engine, static (spotlight, _) => new(spotlight));
        return viewModel;
    }

    private static IFetchEngine<Novel> CreateNovelEngine(IFetchEngine<IWorkEntry> engine) =>
        engine.MakoClient.Computed(SelectNovels(engine));

    private static void OpenWorkRecommendedPage(HomePageCardLayout card, HomeCardPreviewViewModel previewViewModel, TopLevel topLevel)
    {
        topLevel.ViewContainer?.NavigateTo(new WorkRecommendedPage(card.WorkType, previewViewModel.ViewModel.CloneAsWorkViewModel()));
    }

    private static void OpenWorkNewPage(HomePageCardLayout card, HomeCardPreviewViewModel previewViewModel, TopLevel topLevel)
    {
        topLevel.ViewContainer?.NavigateTo(new WorkNewPage(card.WorkType, previewViewModel.ViewModel.CloneAsWorkViewModel()));
    }

    private static void OpenWorkPostsPage(HomePageCardLayout card, HomeCardPreviewViewModel previewViewModel, TopLevel topLevel)
    {
        topLevel.ViewContainer?.NavigateTo(new WorkPostsPage(CreateUserBasicInfo(card), card.WorkType, previewViewModel.ViewModel.CloneAsWorkViewModel()));
    }

    private static void OpenWorkBookmarksPage(HomePageCardLayout card, HomeCardPreviewViewModel previewViewModel, TopLevel topLevel)
    {
        topLevel.ViewContainer?.NavigateTo(new WorkBookmarksPage(
            CreateUserBasicInfo(card),
            card.SimpleWorkType,
            card.PrivacyPolicy,
            card.Tag,
            previewViewModel.ViewModel.CloneAsWorkViewModel()));
    }

    private static void OpenWorkRankingPage(HomePageCardLayout card, HomeCardPreviewViewModel previewViewModel, TopLevel topLevel)
    {
        topLevel.ViewContainer?.NavigateTo(new WorkRankingPage(
            card.SimpleWorkType,
            card.RankOption,
            card.GetRankingDate().LocalDateTime,
            previewViewModel.ViewModel.CloneAsWorkViewModel()));
    }

    private static void OpenWorkFollowingPage(HomePageCardLayout card, HomeCardPreviewViewModel previewViewModel, TopLevel topLevel)
    {
        topLevel.ViewContainer?.NavigateTo(new WorkFollowingPage(card.SimpleWorkType, card.PrivacyPolicy, previewViewModel.ViewModel.CloneAsWorkViewModel()));
    }

    private static void OpenWorkMyPixivPage(HomePageCardLayout card, HomeCardPreviewViewModel previewViewModel, TopLevel topLevel)
    {
        topLevel.ViewContainer?.NavigateTo(new WorkMyPixivPage(card.SimpleWorkType, previewViewModel.ViewModel.CloneAsWorkViewModel()));
    }

    private static void OpenWorkRelatedPage(HomePageCardLayout card, HomeCardPreviewViewModel previewViewModel, TopLevel topLevel)
    {
        topLevel.ViewContainer?.NavigateTo(new WorkRelatedPage(card.EntryId, card.SimpleWorkType, previewViewModel.ViewModel.CloneAsWorkViewModel()));
    }

    private static void OpenWorkSearchPage(HomePageCardLayout card, HomeCardPreviewViewModel previewViewModel, TopLevel topLevel)
    {
        var searchText = card.SearchText ?? "";
        topLevel.ViewContainer?.NavigateTo(new WorkSearchResultPage(
            searchText,
            new IllustrationSearchArguments(searchText),
            new NovelSearchArguments(searchText),
            card.SimpleWorkType,
            previewViewModel.ViewModel.CloneAsWorkViewModel()));
    }

    private static void OpenUserRecommendedPage(HomePageCardLayout card, HomeCardPreviewViewModel previewViewModel, TopLevel topLevel)
    {
        topLevel.ViewContainer?.NavigateTo(new UserRecommendPage(previewViewModel.ViewModel.CloneAsUserViewModel()));
    }

    private static void OpenUserSearchPage(HomePageCardLayout card, HomeCardPreviewViewModel previewViewModel, TopLevel topLevel)
    {
        topLevel.ViewContainer?.NavigateTo(new UserSearchPage(card.SearchText, previewViewModel.ViewModel.CloneAsUserViewModel()));
    }

    private static void OpenUserFollowingPage(HomePageCardLayout card, HomeCardPreviewViewModel previewViewModel, TopLevel topLevel)
    {
        topLevel.ViewContainer?.NavigateTo(new UserFollowingPage(card.UserId, card.PrivacyPolicy, previewViewModel.ViewModel.CloneAsUserViewModel()));
    }

    private static void OpenUserFollowerPage(HomePageCardLayout card, HomeCardPreviewViewModel previewViewModel, TopLevel topLevel)
    {
        topLevel.ViewContainer?.NavigateTo(new UserFollowerPage(previewViewModel.ViewModel.CloneAsUserViewModel()));
    }

    private static void OpenUserMyPixivPage(HomePageCardLayout card, HomeCardPreviewViewModel previewViewModel, TopLevel topLevel)
    {
        topLevel.ViewContainer?.NavigateTo(new UserMyPixivPage(card.UserId, previewViewModel.ViewModel.CloneAsUserViewModel()));
    }

    private static void OpenSpotlightPage(HomePageCardLayout card, HomeCardPreviewViewModel previewViewModel, TopLevel topLevel)
    {
        topLevel.ViewContainer?.NavigateTo(new SpotlightPage(previewViewModel.ViewModel.CloneAsSpotlightViewModel()));
    }

    private static void OpenFirstPreviewItem(HomePageCardLayout card, HomeCardPreviewViewModel previewViewModel, TopLevel topLevel)
    {
        if (previewViewModel.Items.FirstOrDefault() is not { } item)
            return;

        OpenPreviewItem(previewViewModel, topLevel, item);
    }

    private static UserBasicInfo CreateUserBasicInfo(HomePageCardLayout card) =>
        PixevalSettings.Me is { Id: var meId } me && card.UserId == meId
            ? me
            : new HomeCardUserBasicInfo(card.UserId, card.BuildTitle());

    private static void OpenNovel(TopLevel topLevel, ISimpleViewViewModel? sourceViewModel, NovelItemViewModel viewModel)
    {
        if (topLevel.ViewContainer is not { } viewContainer)
            return;

        if (sourceViewModel is NovelViewViewModel novelViewViewModel)
            viewContainer.CreateNovelPage(viewModel, novelViewViewModel.DataProvider.CloneRef());
        else
            viewContainer.CreateNovelPage(viewModel.Entry.Id);
    }

    private static void OpenIllustration(TopLevel topLevel, ISimpleViewViewModel? sourceViewModel, IllustrationItemViewModel viewModel)
    {
        if (topLevel.ViewContainer is not { } viewContainer)
            return;

        if (sourceViewModel is IllustrationViewViewModel illustrationViewViewModel)
        {
            viewContainer.CreateIllustrationPage(viewModel, illustrationViewViewModel.DataProvider.CloneRef());
            return;
        }

        if (viewModel.Entry is Illustration illustration)
            viewContainer.CreateIllustrationPage(illustration);
    }

    private static User CreateUser(UserInfo userInfo) => new()
    {
        UserInfo = userInfo,
        Illustrations = [],
        Novels = [],
        IsMuted = false
    };

    private static async IAsyncEnumerable<T> Single<T>(T item)
    {
        yield return item;
        await Task.CompletedTask;
    }

    private static async IAsyncEnumerable<Novel> SelectNovels(IAsyncEnumerable<IWorkEntry> source)
    {
        await foreach (var item in source)
        {
            if (item is Novel novel)
                yield return novel;
        }
    }

    private static string GetDescription<TEnum>(TEnum value)
        where TEnum : struct, Enum =>
        SymbolComboBoxItem.GetResource(value);

    private static string GetRankOptionDescription(HomePageCardLayout card) =>
        SymbolComboBoxItem.GetResource(card.RankOption, card.SimpleWorkType);
}
