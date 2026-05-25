// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using FluentIcons.Common;
using Mako.Global.Enum;
using Pixeval.Models.Home;

namespace Pixeval.Views.Home;

public sealed record HomeCardTemplate(
    HomePageCardSourceKind SourceKind,
    HomePageCardTemplateKind TemplateKind,
    Symbol Symbol,
    string Title,
    string Description,
    WorkType WorkType = WorkType.Illustration,
    SimpleWorkType SimpleWorkType = SimpleWorkType.IllustrationAndManga,
    PrivacyPolicy PrivacyPolicy = PrivacyPolicy.Public,
    RankOption RankOption = RankOption.Day)
{
    public int DefaultColumnSpan => 2;

    public int DefaultRowSpan => 2;
}
