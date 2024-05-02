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
    : ObservableSettingsEntryBase<AppSettings>(appSettings, "", "", default)
{
    public override DownloadMacroSettingsExpander Element => new() { Entry = this };

    public string DefaultDownloadPathMacro
    {
        get => Settings.DefaultDownloadPathMacro;
        set => Settings.DefaultDownloadPathMacro = value;
    }

    public override void ValueReset() => OnPropertyChanged(nameof(DefaultDownloadPathMacro));

    private static readonly IDictionary<string, string> _macroTooltips = new Dictionary<string, string>
    {
        ["ext"] = SettingsPageResources.ExtMacroTooltip,
        ["id"] = SettingsPageResources.IdMacroTooltip,
        ["title"] = SettingsPageResources.TitleMacroTooltip,
        ["artist_id"] = SettingsPageResources.ArtistIdMacroTooltip,
        ["artist_name"] = SettingsPageResources.ArtistNameMacroTooltip,
        ["if_r18"] = SettingsPageResources.IfR18MacroTooltip,
        ["if_r18g"] = SettingsPageResources.IfR18GMacroTooltip,
        ["if_ai"] = SettingsPageResources.IfAiMacroTooltip,
        ["if_illust"] = SettingsPageResources.IfIllustMacroTooltip,
        ["if_novel"] = SettingsPageResources.IfNovelMacroTooltip,
        ["if_manga"] = SettingsPageResources.IfMangaMacroTooltip,
        ["if_gif"] = SettingsPageResources.IfGifMacroTooltip,
        ["manga_index"] = SettingsPageResources.MangaIndexMacroTooltip
    };

    public static ICollection<StringRepresentableItem> AvailableMacros { get; } = MetaPathMacroAttributeHelper.GetAttachedTypeInstances()
        .Select(m => new StringRepresentableItem(_macroTooltips[m.Name], $"@{{{(m is IPredicate ? $"{m.Name}=" : m.Name)}}}"))
        .ToList();
}
