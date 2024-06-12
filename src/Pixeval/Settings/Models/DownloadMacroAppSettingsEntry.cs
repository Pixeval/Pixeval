using System.Collections.Generic;
using System.Linq;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Controls.Settings;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;

namespace Pixeval.Settings.Models;

public class DownloadMacroAppSettingsEntry(
    AppSettings appSettings)
    : StringAppSettingsEntry(appSettings, t => t.DownloadPathMacro)
{
    public override DownloadMacroSettingsExpander Element => new() { Entry = this };

    private static readonly IReadOnlyDictionary<string, string> _macroTooltips = new Dictionary<string, string>
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
        ["if_illust"] = SettingsPageResources.IfIllustMacroTooltip,
        ["if_novel"] = SettingsPageResources.IfNovelMacroTooltip,
        ["if_manga"] = SettingsPageResources.IfMangaMacroTooltip,
        ["if_gif"] = SettingsPageResources.IfGifMacroTooltip,
        ["manga_index"] = SettingsPageResources.MangaIndexMacroTooltip
    };

    public static ICollection<StringRepresentableItem> AvailableMacros { get; } = MetaPathMacroAttributeHelper.GetIWorkViewModelInstances()
        .Select(m => new StringRepresentableItem(_macroTooltips[m.Name], $"@{{{(m is IPredicate ? $"{m.Name}=" : m.Name)}}}"))
        .ToList();
}
