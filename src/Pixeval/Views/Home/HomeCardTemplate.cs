// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Media;
using FluentIcons.Common;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Models.Home;

namespace Pixeval.Views.Home;

public sealed record HomeCardTemplate(
    HomePageCardSourceKind SourceKind,
    HomePageCardTemplateKind TemplateKind,
    string Title,
    string Description,
    int DefaultColumnSpan,
    int DefaultRowSpan,
    Color Color,
    WorkType WorkType = WorkType.Illustration,
    SimpleWorkType SimpleWorkType = SimpleWorkType.IllustrationAndManga,
    PrivacyPolicy PrivacyPolicy = PrivacyPolicy.Public,
    RankOption RankOption = RankOption.Day,
    SpotlightCategory SpotlightCategory = SpotlightCategory.All)
{
    public IBrush Brush { get; } = new SolidColorBrush(Color);

    public string SizeText => $"{DefaultColumnSpan} x {DefaultRowSpan}";

    public Symbol Symbol => TemplateKind switch
    {
        HomePageCardTemplateKind.WorkList or HomePageCardTemplateKind.NovelList => SourceKind switch
        {
            HomePageCardSourceKind.WorkRecommended => Symbol.Calendar,
            HomePageCardSourceKind.WorkBookmarks => Symbol.Library,
            HomePageCardSourceKind.WorkRanking => Symbol.ArrowTrendingLines,
            HomePageCardSourceKind.WorkNew => Symbol.ArrowSync,
            HomePageCardSourceKind.WorkFollowing => Symbol.PersonHeart,
            HomePageCardSourceKind.WorkPosts => Symbol.AlertUrgent,
            HomePageCardSourceKind.WorkSearch => Symbol.SearchSparkle,
            _ => TemplateKind is HomePageCardTemplateKind.NovelList ? Symbol.BookOpen : Symbol.ImageMultiple
        },
        HomePageCardTemplateKind.UserList => SourceKind switch
        {
            HomePageCardSourceKind.UserRecommended => Symbol.PeopleCommunity,
            HomePageCardSourceKind.UserSearch => Symbol.SearchSparkle,
            HomePageCardSourceKind.UserFollowing => Symbol.PersonHeart,
            HomePageCardSourceKind.UserMyPixiv => Symbol.People,
            _ => Symbol.PeopleCommunity
        },
        HomePageCardTemplateKind.SpotlightList => Symbol.SlideTextSparkle,
        HomePageCardTemplateKind.SingleImage => Symbol.Image,
        HomePageCardTemplateKind.SingleNovel => Symbol.BookOpen,
        HomePageCardTemplateKind.SingleUser => Symbol.Person,
        _ => Symbol.Board
    };
}
