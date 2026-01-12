// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using AutoSettingsPage.Models;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;

namespace Pixeval.Settings.Models;

public partial class DownloadMacroAppSettingsEntry(
    AppSettings settings)
    : StringSettingsEntry<AppSettings>(settings, t => t.DownloadPathMacro)
{
    private static readonly FrozenDictionary<string, string> _MacroTooltips = new Dictionary<string, string>
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
    }.ToFrozenDictionary();

    public static ICollection<StringRepresentableItem> AvailableMacros { get; } = [.. MetaPathMacroAttributeHelper.GetIArtworkInfoInstances().Select(m => new StringRepresentableItem(_MacroTooltips[m.Name], $"@{{{(m is IPredicate ? $"{m.Name}=" : m.Name)}}}"))];
}
