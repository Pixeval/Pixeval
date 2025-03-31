// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Controls.Settings;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;

namespace Pixeval.Settings.Models;

public partial class DownloadMacroAppSettingsEntry(
    AppSettings settings)
    : StringAppSettingsEntry(settings, t => t.DownloadPathMacro)
{
    public override DownloadMacroSettingsExpander Element => new() { Entry = this };

    private static readonly ImmutableDictionary<string, string> _MacroTooltips = new Dictionary<string, string>
    {
        ["ext"] = SettingsPageResources.ExtMacroTooltip,
        ["id"] = SettingsPageResources.IdMacroTooltip,
        ["title"] = SettingsPageResources.TitleMacroTooltip,
        ["artist_id"] = SettingsPageResources.ArtistIdMacroTooltip,
        ["artist_name"] = SettingsPageResources.ArtistNameMacroTooltip,
        ["publish_year"] = SettingsPageResources.PublishYearMacroTooltip,
        ["publish_month"] = SettingsPageResources.PublishMonthMacroTooltip,
        ["publish_day"] = SettingsPageResources.PublishDayMacroTooltip,
        ["if_r18"] = SettingsPageResources.IfR18MacroTooltip,
        ["if_r18g"] = SettingsPageResources.IfR18GMacroTooltip,
        ["if_ai"] = SettingsPageResources.IfAiMacroTooltip,
        ["if_novel"] = SettingsPageResources.IfNovelMacroTooltip,
        ["if_pic_one"] = SettingsPageResources.IfPicOneMacroTooltip,
        ["if_pic_set"] = SettingsPageResources.IfPicSetMacroTooltip,
        ["if_pic_gif"] = SettingsPageResources.IfPicGifMacroTooltip,
        ["if_pic_all"] = SettingsPageResources.IfPicAllMacroTooltip,
        ["pic_set_index"] = SettingsPageResources.PicSetIndexMacroTooltip
    }.ToImmutableDictionary();

    public static ICollection<StringRepresentableItem> AvailableMacros { get; } = [.. MetaPathMacroAttributeHelper.GetIWorkViewModelInstances().Select(m => new StringRepresentableItem(_MacroTooltips[m.Name], $"@{{{(m is IPredicate ? $"{m.Name}=" : m.Name)}}}"))];
}
