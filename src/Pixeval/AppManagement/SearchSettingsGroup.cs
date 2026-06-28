// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Text.Json.Serialization;
using AutoSettingsPage;
using FluentIcons.Common;
using Mako.Global.Enum;

namespace Pixeval.AppManagement;

public record SearchSettingsGroup
{
    [SettingsEntry(
        Symbol.Key,
        AppSettingsResources.ReverseSearchApiKeyEntryHeader,
        AppSettingsResources.ReverseSearchApiKeyEntryDescription,
        AppSettingsResources.ReverseSearchApiKeyEntryPlaceholder,
        DescriptionLink = "https://saucenao.com/user.php?page=search-api")]
    public string ReverseSearchApiKey { get; set; } = "";

    [SettingsEntry(Symbol.Grid, AppSettingsResources.SimpleWorkTypeEntryHeader, AppSettingsResources.SimpleWorkTypeEntryDescription)]
    public SimpleWorkType DefaultSimpleWorkType { get; set; }

    [SettingsEntry(Symbol.ArrowTrending, AppSettingsResources.RankOptionEntryHeader, AppSettingsResources.RankOptionEntryDescription)]
    public RankOption IllustrationRankOption { get; set; }

    public RankOption NovelRankOption { get; set; }

    [JsonIgnore]
    public WorkType WorkType => DefaultSimpleWorkType is SimpleWorkType.IllustrationAndManga
        ? WorkType.Illustration
        : WorkType.Novel;
}
