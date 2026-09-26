// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Text.Json.Serialization;
using AutoSettingsPage;
using FluentIcons.Common;
using Mako.Global.Enum;

namespace Pixeval.AppManagement.Settings;

public record SearchSettingsGroup
{
    [SettingsEntry(Symbol.ArrowTrending, AppSettingsResources.RankOptionEntry.Header, AppSettingsResources.RankOptionEntry.Description)]
    public RankOptionsSettings RankOptions { get; set; } = new();

    [SettingsEntry(
        Symbol.Key,
        AppSettingsResources.SauceNaoApiKeyEntry.Header,
        AppSettingsResources.SauceNaoApiKeyEntry.Description,
        AppSettingsResources.SauceNaoApiKeyEntry.Placeholder,
        DescriptionLink = "https://saucenao.com/user.php?page=search-api")]
    public string SauceNaoApiKey { get; set; } = "";

    [SettingsEntry(Symbol.Grid, AppSettingsResources.SimpleWorkTypeEntry.Header, AppSettingsResources.SimpleWorkTypeEntry.Description)]
    public SimpleWorkType DefaultSimpleWorkType { get; set; }

    [JsonIgnore]
    public WorkType WorkType => DefaultSimpleWorkType is SimpleWorkType.Illustration
        ? WorkType.Illustration
        : WorkType.Novel;
}
