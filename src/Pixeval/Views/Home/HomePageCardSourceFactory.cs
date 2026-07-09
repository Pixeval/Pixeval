// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            HomePageCardTemplateKind.WorkList => CreateWorkViewModel(card),
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
            HomePageCardSourceKind.WorkRecommended => client.WorkRecommended(card.WorkType),
            HomePageCardSourceKind.WorkBookmarks => client.WorkBookmarks(card.UserId, card.SimpleWorkType, card.PrivacyPolicy, card.Tag),
            HomePageCardSourceKind.WorkRanking => client.WorkRanking(card.SimpleWorkType, card.RankOption, card.GetRankingDate()),
            HomePageCardSourceKind.WorkNew => client.WorkNew(card.WorkType),
            HomePageCardSourceKind.WorkFollowing => client.WorkFollowing(card.SimpleWorkType, card.PrivacyPolicy),
            HomePageCardSourceKind.WorkMyPixiv => client.WorkMyPixiv(card.SimpleWorkType),
            HomePageCardSourceKind.WorkRelated => client.WorkRelated(card.EntryId, card.SimpleWorkType),
            HomePageCardSourceKind.WorkPosts => client.WorkPosted(card.UserId, card.WorkType),
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
            HomePageCardSourceKind.WorkRecommended => CreateNovelEngine(client.WorkRecommended(WorkType.Novel)),
            HomePageCardSourceKind.WorkBookmarks => CreateNovelEngine(client.WorkBookmarks(card.UserId, SimpleWorkType.Novel, card.PrivacyPolicy, card.Tag)),
            HomePageCardSourceKind.WorkRanking => CreateNovelEngine(client.WorkRanking(SimpleWorkType.Novel, card.RankOption, card.GetRankingDate())),
            HomePageCardSourceKind.WorkNew => CreateNovelEngine(client.WorkNew(WorkType.Novel)),
            HomePageCardSourceKind.WorkFollowing => CreateNovelEngine(client.WorkFollowing(SimpleWorkType.Novel, card.PrivacyPolicy)),
            HomePageCardSourceKind.WorkMyPixiv => CreateNovelEngine(client.WorkMyPixiv(SimpleWorkType.Novel)),
            HomePageCardSourceKind.WorkRelated => CreateNovelEngine(client.WorkRelated(card.EntryId, SimpleWorkType.Novel)),
            HomePageCardSourceKind.WorkPosts => CreateNovelEngine(client.WorkPosted(card.UserId, WorkType.Novel)),
            HomePageCardSourceKind.WorkSearch => string.IsNullOrWhiteSpace(card.SearchText)
                ? client.Computed(AsyncEnumerable.Empty<Novel>())
                : client.NovelSearch(new NovelSearchArguments(card.SearchText)),
            _ => client.Computed(AsyncEnumerable.Empty<Novel>())
        };
    }

    private static IFetchEngine<Novel> CreateNovelEngine(IFetchEngine<IWorkEntry> engine) =>
        engine.MakoClient.Computed(SelectNovels(engine));

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
            HomePageCardSourceKind.UserFollower => client.UserFollower(),
            HomePageCardSourceKind.UserMyPixiv => client.UserMyPixiv(card.UserId),
            _ => client.Computed(AsyncEnumerable.Empty<User>())
        };
    }

    private static bool UsesNovelEngine(HomePageCardLayout card) =>
        card.SourceKind switch
        {
            HomePageCardSourceKind.WorkRecommended or HomePageCardSourceKind.WorkNew or HomePageCardSourceKind.WorkPosts => card.WorkType is WorkType.Novel,
            HomePageCardSourceKind.WorkBookmarks or HomePageCardSourceKind.WorkRanking or HomePageCardSourceKind.WorkFollowing
                or HomePageCardSourceKind.WorkMyPixiv or HomePageCardSourceKind.WorkRelated or HomePageCardSourceKind.WorkSearch => card.SimpleWorkType is SimpleWorkType.Novel,
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

    private static async IAsyncEnumerable<Novel> SelectNovels(IAsyncEnumerable<IWorkEntry> source)
    {
        await foreach (var item in source)
        {
            if (item is Novel novel)
                yield return novel;
        }
    }
}
