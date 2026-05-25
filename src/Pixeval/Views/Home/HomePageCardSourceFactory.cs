// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading.Tasks;
using Mako;
using Mako.Engine;
using Mako.Engine.Implements;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Models.Home;

namespace Pixeval.Views.Home;

public static class HomePageCardSourceFactory
{
    public static async Task LoadPreviewItemsAsync(HomeCardPreviewViewModel viewModel)
    {
        var card = viewModel.Card;
        switch (card.TemplateKind)
        {
            case HomePageCardTemplateKind.WorkList or HomePageCardTemplateKind.NovelList:
                var workEngine = CreateWorkEngine(card);
                viewModel.SetEngine(workEngine, (work, index) => work is Novel novel
                    ? new HomeCardNovelPreviewItem(novel)
                    : new HomeCardImagePreviewItem(work));
                return;
            case HomePageCardTemplateKind.UserList:
                var userEngine = CreateUserEngine(card);
                viewModel.SetEngine(userEngine, (user, index) => new HomeCardUserPreviewItem(user.UserInfo));
                return;
            case HomePageCardTemplateKind.SpotlightList:
                var spotlightEngine = App.AppViewModel.MakoClient.Spotlight();
                viewModel.SetEngine(spotlightEngine, (spotlight, index) => new HomeCardSpotlightPreviewItem(spotlight));
                return;
            case HomePageCardTemplateKind.SingleImage:
                viewModel.SetItems(new HomeCardImagePreviewItem(await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(card.EntryId)));
                return;
            case HomePageCardTemplateKind.SingleNovel:
                viewModel.SetItems(new HomeCardNovelPreviewItem(await App.AppViewModel.MakoClient.GetNovelFromIdAsync(card.EntryId)));
                return;
            case HomePageCardTemplateKind.SingleUser:
                viewModel.SetItems(new HomeCardUserPreviewItem((await App.AppViewModel.MakoClient.GetUserFromIdAsync(card.UserId)).UserEntity));
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(card));
        }
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
}
