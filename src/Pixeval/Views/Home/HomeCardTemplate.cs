// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using FluentIcons.Common;
using Mako.Global.Enum;
using Pixeval.Controls;
using Pixeval.Models.Options;

namespace Pixeval.Views.Home;

public sealed record HomeCardTemplate(
    HomePageCardSourceKind SourceKind,
    WorkType WorkType = WorkType.Illustration,
    SimpleWorkType SimpleWorkType = SimpleWorkType.Illustration,
    PrivacyPolicy PrivacyPolicy = PrivacyPolicy.Public,
    RankOption RankOption = RankOption.Day)
{
    public int DefaultColumnSpan => 2;

    public int DefaultRowSpan => 2;

    public string Title => SymbolComboBoxItem.GetResource(SourceKind);

    public string Description => SymbolComboBoxItem.GetResource(SourceKind, nameof(Description));

    public Symbol Symbol => SourceKind switch
    {
        HomePageCardSourceKind.WorkRecommended => Symbol.Calendar,
        HomePageCardSourceKind.WorkBookmarks => Symbol.Library,
        HomePageCardSourceKind.WorkRanking => Symbol.ArrowTrendingLines,
        HomePageCardSourceKind.WorkNew => Symbol.ArrowSync,
        HomePageCardSourceKind.WorkFollowing => Symbol.AlertUrgent,
        HomePageCardSourceKind.WorkMyPixiv => Symbol.Molecule,
        HomePageCardSourceKind.WorkRelated => Symbol.LightbulbFilament,
        HomePageCardSourceKind.WorkPosts => Symbol.Image,
        HomePageCardSourceKind.WorkSearch => Symbol.SearchSparkle,
        HomePageCardSourceKind.UserRecommended => Symbol.PeopleCommunity,
        HomePageCardSourceKind.UserSearch => Symbol.SearchSparkle,
        HomePageCardSourceKind.UserFollowing => Symbol.PersonHeart,
        HomePageCardSourceKind.UserFollower => Symbol.People,
        HomePageCardSourceKind.UserMyPixiv => Symbol.PeopleInterwoven,
        HomePageCardSourceKind.Spotlight => Symbol.SlideTextSparkle,
        HomePageCardSourceKind.SingleImage => Symbol.Image,
        HomePageCardSourceKind.SingleNovel => Symbol.BookOpen,
        HomePageCardSourceKind.SingleUser => Symbol.Person,
        _ => throw new ArgumentOutOfRangeException(nameof(SourceKind), SourceKind, null)
    };
}
