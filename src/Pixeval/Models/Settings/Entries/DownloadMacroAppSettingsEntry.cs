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

namespace Pixeval.Models.Settings.Entries;

public class DownloadMacroAppSettingsEntry(
    AppSettings settings)
    : StringSettingsEntry<AppSettings>(settings, t => t.DownloadPathMacro)
{
    private static readonly FrozenDictionary<string, string> _MacroTooltips = new Dictionary<string, string>
    {
        ["ext"] = SettingsMainViewResources.ExtMacroTooltip,
        ["id"] = SettingsMainViewResources.IdMacroTooltip,
        ["title"] = SettingsMainViewResources.TitleMacroTooltip,
        ["artist_id"] = SettingsMainViewResources.ArtistIdMacroTooltip,
        ["artist_name"] = SettingsMainViewResources.ArtistNameMacroTooltip,
        ["publish_year"] = SettingsMainViewResources.PublishYearMacroTooltip,
        ["publish_month"] = SettingsMainViewResources.PublishMonthMacroTooltip,
        ["publish_day"] = SettingsMainViewResources.PublishDayMacroTooltip,
        ["if_r18"] = SettingsMainViewResources.IfR18MacroTooltip,
        ["if_r18g"] = SettingsMainViewResources.IfR18GMacroTooltip,
        ["if_ai"] = SettingsMainViewResources.IfAiMacroTooltip,
        ["if_novel"] = SettingsMainViewResources.IfNovelMacroTooltip,
        ["if_pic_one"] = SettingsMainViewResources.IfPicOneMacroTooltip,
        ["if_pic_set"] = SettingsMainViewResources.IfPicSetMacroTooltip,
        ["if_pic_gif"] = SettingsMainViewResources.IfPicGifMacroTooltip,
        ["pic_set_index"] = SettingsMainViewResources.PicSetIndexMacroTooltip
    }.ToFrozenDictionary();

    public static ICollection<SymbolComboBoxItem> AvailableMacros { get; } = [.. MetaPathMacroAttributeHelper.GetIArtworkInfoInstances().Select(m => new SymbolComboBoxItem($"@{{{(m is IPredicate ? $"{m.Name}?:" : m.Name)}}}", _MacroTooltips[m.Name], default))];
}
