// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mako;
using Mako.Engine;
using Mako.Engine.Implements;
using Mako.Global.Enum;
using Mako.Model;
using Misaki;
using Pixeval.Models.Home;
using Pixeval.Models.Options;
using Pixeval.ViewModels;

namespace Pixeval.Views.Home;

public static class HomePageCardSourceFactory
{
    public static async Task<ISimpleViewViewModel> CreateViewModelAsync(HomePageCardLayout card)
    {
        var client = App.AppViewModel.MakoClient;
        return card.TemplateKind switch
        {
            HomePageCardTemplateKind.WorkList or HomePageCardTemplateKind.NovelList => CreateWorkViewModel(card),
            HomePageCardTemplateKind.UserList => CreateUserViewModel(CreateUserEngine(card)),
            HomePageCardTemplateKind.SpotlightList => CreateSpotlightViewModel(client.Spotlight()),
            HomePageCardTemplateKind.SingleImage => CreateIllustrationViewModel(
                client.Computed(Single(await client.GetIllustrationFromIdAsync(card.EntryId)))),
            HomePageCardTemplateKind.SingleNovel => CreateNovelViewModel(
                client.Computed(Single(await client.GetNovelFromIdAsync(card.EntryId)))),
            HomePageCardTemplateKind.SingleUser => CreateUserViewModel(
                client.Computed(Single(CreateUser((await client.GetUserFromIdAsync(card.UserId)).UserEntity)))),
            _ => throw new ArgumentOutOfRangeException(nameof(card))
        };
    }

    private static ISimpleViewViewModel CreateWorkViewModel(HomePageCardLayout card) =>
        UsesNovelEngine(card)
            ? CreateNovelViewModel(CreateNovelEngine(card))
            : CreateIllustrationViewModel(CreateIllustrationEngine(card));

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

    private static IFetchEngine<IArtworkInfo> CreateIllustrationEngine(HomePageCardLayout card)
    {
        var client = App.AppViewModel.MakoClient;
        return card.SourceKind switch
        {
            HomePageCardSourceKind.WorkRecommended => card.WorkType is WorkType.Manga
                ? client.MangaRecommended()
                : client.IllustrationRecommended(),
            HomePageCardSourceKind.WorkBookmarks => client.IllustrationBookmarks(card.UserId, card.Tag, card.PrivacyPolicy),
            HomePageCardSourceKind.WorkRanking => client.IllustrationRanking(card.RankOption, card.GetRankingDate()),
            HomePageCardSourceKind.WorkNew => client.IllustrationNew(card.WorkType is WorkType.Manga, null),
            HomePageCardSourceKind.WorkFollowing => client.IllustrationFollowing(card.PrivacyPolicy),
            HomePageCardSourceKind.WorkPosts => client.IllustrationPosted(card.UserId, card.WorkType is WorkType.Manga ? WorkType.Manga : WorkType.Illustration),
            HomePageCardSourceKind.WorkSearch => string.IsNullOrWhiteSpace(card.SearchText)
                ? client.Computed(AsyncEnumerable.Empty<IArtworkInfo>())
                : client.IllustrationSearch(new IllustrationSearchArguments(card.SearchText)),
            _ => client.Computed(AsyncEnumerable.Empty<IArtworkInfo>())
        };
    }

    private static IFetchEngine<Novel> CreateNovelEngine(HomePageCardLayout card)
    {
        var client = App.AppViewModel.MakoClient;
        return card.SourceKind switch
        {
            HomePageCardSourceKind.WorkRecommended => client.NovelRecommended(),
            HomePageCardSourceKind.WorkBookmarks => client.NovelBookmarks(card.UserId, card.Tag, card.PrivacyPolicy),
            HomePageCardSourceKind.WorkRanking => client.NovelRanking(card.RankOption, card.GetRankingDate()),
            HomePageCardSourceKind.WorkNew => client.NovelNew(null),
            HomePageCardSourceKind.WorkFollowing => client.NovelFollowing(card.PrivacyPolicy),
            HomePageCardSourceKind.WorkPosts => client.NovelPosted(card.UserId),
            HomePageCardSourceKind.WorkSearch => string.IsNullOrWhiteSpace(card.SearchText)
                ? client.Computed(AsyncEnumerable.Empty<Novel>())
                : client.NovelSearch(new NovelSearchArguments(card.SearchText)),
            _ => client.Computed(AsyncEnumerable.Empty<Novel>())
        };
    }

    private static IFetchEngine<User> CreateUserEngine(HomePageCardLayout card)
    {
        var client = App.AppViewModel.MakoClient;
        return card.SourceKind switch
        {
            HomePageCardSourceKind.UserRecommended => client.UserRecommended(),
            HomePageCardSourceKind.UserSearch => string.IsNullOrWhiteSpace(card.SearchText)
                ? client.Computed(AsyncEnumerable.Empty<User>())
                : client.UserSearch(card.SearchText),
            HomePageCardSourceKind.UserFollowing => client.UserFollowing(card.UserId, card.PrivacyPolicy),
            HomePageCardSourceKind.UserMyPixiv => client.UserMyPixiv(card.UserId),
            _ => client.Computed(AsyncEnumerable.Empty<User>())
        };
    }

    private static bool UsesNovelEngine(HomePageCardLayout card) =>
        card.TemplateKind is HomePageCardTemplateKind.NovelList or HomePageCardTemplateKind.SingleNovel
        || card.SourceKind switch
        {
            HomePageCardSourceKind.WorkRecommended or HomePageCardSourceKind.WorkNew or HomePageCardSourceKind.WorkPosts => card.WorkType is WorkType.Novel,
            HomePageCardSourceKind.WorkBookmarks or HomePageCardSourceKind.WorkRanking or HomePageCardSourceKind.WorkFollowing or HomePageCardSourceKind.WorkSearch => card.SimpleWorkType is SimpleWorkType.Novel,
            _ => false
        };

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

}
