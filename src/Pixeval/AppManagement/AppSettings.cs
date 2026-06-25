// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;
using AutoSettingsPage;
using Avalonia;
using Avalonia.Styling;
using FluentIcons.Common;
using Mako;
using Pixeval.I18N;
using Pixeval.Models.Options;
using SharpYaml.Serialization;
using static Pixeval.AppSettingsResources;

namespace Pixeval.AppManagement;

public record AppSettings
{
    public AppSettings()
    {
        var defaultFont = I18NManager.GetResource(CultureDefaultSettingsResources.AppDefaultFontFamilyName);
        ApplicationSettings.AppFontFamily = [defaultFont];
        NovelSettings.NovelFontFamily = [defaultFont];
        NovelSettings.ActualThemeProvider = () => ActualTheme;
    }

    [SettingsEntry(Symbol.Apps, SettingsGroupApplicationHeader, null)]
    public ApplicationSettingsGroup ApplicationSettings { get; set; } = new();

    [SettingsEntry(Symbol.WiFi, SettingsGroupNetworkHeader, null)]
    public NetworkSettingsGroup NetworkSettings { get; set; } = new();

    [SettingsEntry(Symbol.News, SettingsGroupBrowsingExperienceHeader, null)]
    public BrowsingExperienceSettingsGroup BrowsingExperienceSettings { get; set; } = new();

    [SettingsEntry(Symbol.SearchSparkle, SettingsGroupSearchHeader, null)]
    public SearchSettingsGroup SearchSettings { get; set; } = new();

    [SettingsEntry(Symbol.ArrowSquareDown, SettingsGroupDownloadHeader, null)]
    public DownloadSettingsGroup DownloadSettings { get; set; } = new();

    [SettingsEntry(Symbol.BookOpen, EntryViewerPageResources.NovelSettings, null)]
    public NovelSettingsGroup NovelSettings { get; set; } = new();

    /// <summary>
    /// <see cref="object"/> 只能是基元类型或 <see cref="SettingsSerializerContext"/> 提供过的类型
    /// </summary>
    public Dictionary<string, Dictionary<string, object?>> ExtensionSettings { get; set; } = [];

    [JsonIgnore]
    public ApplicationTheme ActualTheme => ApplicationSettings.Theme is ApplicationTheme.Default
        ? Application.Current!.ActualThemeVariant == ThemeVariant.Dark ? ApplicationTheme.Dark : ApplicationTheme.Light
        : ApplicationSettings.Theme;

    public MakoConfiguration ToMakoConfiguration()
    {
        return new MakoConfiguration(
            NetworkSettings.EnableDomainFronting,
            NetworkSettings.DomainFrontingType,
            NetworkSettings.Proxy,
            NetworkSettings.WebCookie,
            NetworkSettings.MirrorHost,
            BrowsingExperienceSettings.TargetFilter,
            700,
            CultureInfo.CurrentCulture);
    }
}

[YamlSerializable(typeof(AppSettings))]
[YamlSerializable(typeof(LoginContext))]
public partial class SettingsSerializerContext : YamlSerializerContext;
