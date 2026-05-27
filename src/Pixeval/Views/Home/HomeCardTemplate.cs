// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using FluentIcons.Common;
using Mako.Global.Enum;
using Pixeval.Controls;
using Pixeval.Models.Options;

namespace Pixeval.Views.Home;

public sealed record HomeCardTemplate(
    HomePageCardSourceKind SourceKind,
    HomePageCardTemplateKind TemplateKind,
    Symbol Symbol,
    WorkType WorkType = WorkType.Illustration,
    SimpleWorkType SimpleWorkType = SimpleWorkType.IllustrationAndManga,
    PrivacyPolicy PrivacyPolicy = PrivacyPolicy.Public,
    RankOption RankOption = RankOption.Day)
{
    public int DefaultColumnSpan => 2;

    public int DefaultRowSpan => 2;

    public string Title => SymbolComboBoxItem.GetResource(SourceKind);

    public string Description => SymbolComboBoxItem.GetResource(SourceKind, nameof(Description));
}
