// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentIcons.Common;
using Mako;
using Mako.Engine;
using Mako.Engine.Implements;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Models.Home;
using Pixeval.Utilities;

namespace Pixeval.Views.Home;

public static class HomePageCardSourceFactory
{
    public static async Task<IReadOnlyList<HomeCardPreviewItem>> LoadPreviewItemsAsync(HomePageCardLayout card, int count)
    {
        return card.TemplateKind switch
        {
            HomePageCardTemplateKind.WorkList or HomePageCardTemplateKind.NovelList => await LoadWorkItemsAsync(CreateWorkEngine(card), count),
            HomePageCardTemplateKind.UserList => await LoadUserItemsAsync(CreateUserEngine(card), count),
            HomePageCardTemplateKind.SpotlightList => await LoadSpotlightItemsAsync(App.AppViewModel.MakoClient.Spotlight(card.SpotlightCategory), count),
            HomePageCardTemplateKind.SingleImage => [CreateWorkItem(await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(card.EntryId))],
            HomePageCardTemplateKind.SingleNovel => [CreateWorkItem(await App.AppViewModel.MakoClient.GetNovelFromIdAsync(card.EntryId))],
            HomePageCardTemplateKind.SingleUser => [CreateUserItem((await App.AppViewModel.MakoClient.GetUserFromIdAsync(card.UserId)).UserEntity)],
            _ => throw new ArgumentOutOfRangeException(nameof(card))
        };
    }

    private static IFetchEngine<IWorkEntry> CreateWorkEngine(HomePageCardLayout card)
    {
        var client = App.AppViewModel.MakoClient;
        return card.SourceKind switch
        {
            HomePageCardSourceKind.WorkRecommended => client.WorkRecommended(card.WorkType),
            HomePageCardSourceKind.WorkBookmarks => client.WorkBookmarks(card.UserId, card.SimpleWorkType, card.PrivacyPolicy, card.Tag),
            HomePageCardSourceKind.WorkRanking => client.WorkRanking(card.SimpleWorkType, card.RankOption, GetRankingDate(card)),
            HomePageCardSourceKind.WorkNew => client.WorkNew(card.WorkType),
            HomePageCardSourceKind.WorkFollowing => client.WorkFollowing(card.SimpleWorkType, card.PrivacyPolicy),
            HomePageCardSourceKind.WorkPosts => client.WorkPosts(card.UserId, card.WorkType),
            HomePageCardSourceKind.WorkSearch => string.IsNullOrWhiteSpace(card.SearchText)
                ? client.Computed(System.Linq.AsyncEnumerable.Empty<IWorkEntry>())
                : card.SimpleWorkType is SimpleWorkType.Novel
                    ? client.NovelSearch(new NovelSearchArguments(card.SearchText))
                    : client.IllustrationSearch(new IllustrationSearchArguments(card.SearchText)),
            _ => client.Computed(System.Linq.AsyncEnumerable.Empty<IWorkEntry>())
        };
    }

    private static IFetchEngine<User> CreateUserEngine(HomePageCardLayout card)
    {
        var client = App.AppViewModel.MakoClient;
        return card.SourceKind switch
        {
            HomePageCardSourceKind.UserRecommended => client.UserRecommended(),
            HomePageCardSourceKind.UserSearch => string.IsNullOrWhiteSpace(card.SearchText)
                ? client.Computed(System.Linq.AsyncEnumerable.Empty<User>())
                : client.UserSearch(card.SearchText),
            HomePageCardSourceKind.UserFollowing => client.UserFollowing(card.UserId, card.PrivacyPolicy),
            HomePageCardSourceKind.UserMyPixiv => client.UserMyPixiv(card.UserId),
            _ => client.Computed(System.Linq.AsyncEnumerable.Empty<User>())
        };
    }

    private static DateTimeOffset GetRankingDate(HomePageCardLayout card) =>
        card.RankingDate == default ? MakoClient.RankingMaxDateTime : card.RankingDate;

    private static async Task<IReadOnlyList<HomeCardPreviewItem>> LoadWorkItemsAsync(IAsyncEnumerable<IWorkEntry> engine, int count)
    {
        var items = new List<HomeCardPreviewItem>(count);
        await using var enumerator = engine.GetAsyncEnumerator(CancellationToken.None);
        while (await enumerator.MoveNextAsync())
        {
            items.Add(CreateWorkItem(enumerator.Current));
            if (items.Count >= count)
                break;
        }

        return items;
    }

    private static async Task<IReadOnlyList<HomeCardPreviewItem>> LoadUserItemsAsync(IAsyncEnumerable<User> engine, int count)
    {
        var items = new List<HomeCardPreviewItem>(count);
        await using var enumerator = engine.GetAsyncEnumerator(CancellationToken.None);
        while (await enumerator.MoveNextAsync())
        {
            items.Add(CreateUserItem(enumerator.Current));
            if (items.Count >= count)
                break;
        }

        return items;
    }

    private static async Task<IReadOnlyList<HomeCardPreviewItem>> LoadSpotlightItemsAsync(IAsyncEnumerable<Spotlight> engine, int count)
    {
        var items = new List<HomeCardPreviewItem>(count);
        await using var enumerator = engine.GetAsyncEnumerator(CancellationToken.None);
        while (await enumerator.MoveNextAsync())
        {
            var spotlight = enumerator.Current;
            items.Add(new(spotlight.PureTitle, spotlight.SubcategoryLabel, spotlight.Thumbnail, Symbol.SlideTextSparkle));
            if (items.Count >= count)
                break;
        }

        return items;
    }

    private static HomeCardPreviewItem CreateWorkItem(IWorkEntry entry) => new(
        entry.Title,
        entry.User.Name,
        entry.GetThumbnailUrl(),
        entry is Novel ? Symbol.BookOpen : Symbol.Image);

    private static HomeCardPreviewItem CreateUserItem(User user) => CreateUserItem(user.UserInfo);

    private static HomeCardPreviewItem CreateUserItem(UserInfo user) => new(
        user.Name,
        user.Account,
        string.IsNullOrWhiteSpace(user.AvatarUrl) ? null : user.AvatarUrl,
        Symbol.Person);
}
