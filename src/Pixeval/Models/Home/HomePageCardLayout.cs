// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Mako.Global.Enum;

namespace Pixeval.Models.Home;

public enum HomePageCardTemplateKind
{
    WorkList,
    NovelList,
    UserList,
    SpotlightList,
    SingleImage,
    SingleNovel,
    SingleUser
}

public enum HomePageCardSourceKind
{
    WorkRecommended,
    WorkBookmarks,
    WorkRanking,
    WorkNew,
    WorkFollowing,
    WorkPosts,
    WorkSearch,
    UserRecommended,
    UserSearch,
    UserFollowing,
    UserMyPixiv,
    Spotlight,
    SingleImage,
    SingleNovel,
    SingleUser
}

public record HomePageCardLayout
{
    public HomePageCardTemplateKind TemplateKind { get; set; }

    public HomePageCardSourceKind SourceKind { get; set; }

    public WorkType WorkType { get; set; } = WorkType.Illustration;

    public SimpleWorkType SimpleWorkType { get; set; } = SimpleWorkType.IllustrationAndManga;

    public PrivacyPolicy PrivacyPolicy { get; set; } = PrivacyPolicy.Public;

    public RankOption RankOption { get; set; } = RankOption.Day;

    public long UserId { get; set; }

    public long EntryId { get; set; }

    public string? SearchText { get; set; }

    public string? Tag { get; set; }

    public uint BackgroundColor { get; set; }

    public DateTimeOffset RankingDate { get; set; }

    public int Column { get; set; }

    public int Row { get; set; }

    public int ColumnSpan { get; set; } = 1;

    public int RowSpan { get; set; } = 1;
}
