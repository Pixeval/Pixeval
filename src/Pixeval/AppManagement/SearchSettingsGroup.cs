// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Text.Json.Serialization;
using AutoSettingsPage;
using FluentIcons.Common;
using Mako.Global.Enum;
using static Pixeval.AppSettingsResources;

namespace Pixeval.AppManagement;

public record SearchSettingsGroup
{
    [SettingsEntry(
        Symbol.Key,
        SauceNaoApiKeyEntryHeader,
        SauceNaoApiKeyEntryDescription,
        SauceNaoApiKeyEntryPlaceholder,
        DescriptionLink = "https://saucenao.com/user.php?page=search-api")]
    public string SauceNaoApiKey { get; set; } = "";

    [SettingsEntry(Symbol.Grid, SimpleWorkTypeEntryHeader, SimpleWorkTypeEntryDescription)]
    public SimpleWorkType DefaultSimpleWorkType { get; set; }

    [SettingsEntry(Symbol.ArrowTrending, RankOptionEntryHeader, RankOptionEntryDescription)]
    public RankOption IllustrationRankOption { get; set; }

    public RankOption NovelRankOption { get; set; }

    [JsonIgnore]
    public WorkType WorkType => DefaultSimpleWorkType is SimpleWorkType.Illustration
        ? WorkType.Illustration
        : WorkType.Novel;
}
