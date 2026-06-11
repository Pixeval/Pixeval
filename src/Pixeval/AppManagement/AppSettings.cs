// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoSettingsPage;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using FluentIcons.Common;
using Mako;
using Mako.Global.Enum;
using Mako.Net;
using Pixeval.I18N;
using Pixeval.Models.Home;
using Pixeval.Models.Options;
using static Pixeval.AppSettingsResources;

namespace Pixeval.AppManagement;

public record AppSettings
{
    public AppSettings()
    {
        var defaultFont = I18NManager.GetResource(CultureDefaultSettingsResources.AppDefaultFontFamilyName);
        AppFontFamily =  NovelFontFamily = [defaultFont];
    }

    [Obsolete]
    [SettingsEntry(Symbol.Communication, DownloadUpdateAutomaticallyEntryHeader, DownloadUpdateAutomaticallyEntryDescription)]
    public bool DownloadUpdateAutomatically { get; set; }

    /// <summary>
    /// "" 表示使用系统默认语言
    /// </summary>
    [SettingsEntry(Symbol.LocalLanguage, AppLanguageEntryHeader, AppLanguageEntryDescription, AppLanguageEntryPlaceholder)]
    public string CultureName { get; set; } = "";

    /// <summary>
    /// The Application Theme
    /// </summary>
    [SettingsEntry(Symbol.DarkTheme, ThemeEntryHeader, ThemeEntryDescription)]
    public ApplicationTheme Theme { get; set; }

    [SettingsEntry(Symbol.ShieldTask, EnableDomainFrontingEntryHeader, EnableDomainFrontingEntryDescription)]
    public bool EnableDomainFronting { get; set; } = true;

    [SettingsEntry(Symbol.ShieldSettings, DomainFrontingTypeEntryHeader, DomainFrontingTypeEntryDescription)]
    public DomainFrontingType DomainFrontingType { get; set; } = DomainFrontingType.Fragmentation;

    [SettingsEntry(Symbol.Database, UseFileCacheEntryHeader, UseFileCacheEntryDescription)]
    public bool UseFileCache { get; set; } = true;

    [SettingsEntry(Symbol.TextFont, AppFontFamilyEntryHeader, AppFontFamilyEntryDescription, AppFontFamilyEntryPlaceholder)]
    public ObservableCollection<string> AppFontFamily { get; set; }

    [SettingsEntry(Symbol.Table, HomePageRowsEntryHeader, HomePageRowsEntryDescription)]
    public int HomePageRows { get; set; } = 7;

    [SettingsEntry(Symbol.Table, HomePageColumnsEntryHeader, HomePageColumnsEntryDescription)]
    public int HomePageColumns { get; set; } = 1;

    [SettingsEntry(Symbol.WindowHeaderHorizontal, HideHomePageToolbarEntryHeader, HideHomePageToolbarEntryDescription)]
    public bool HideHomePageToolbar { get; set; }

    [SettingsEntry(Symbol.AppTitle, HideHomePageCardTitleEntryHeader, HideHomePageCardTitleEntryDescription)]
    public bool HideHomePageCardTitle { get; set; }

    public ObservableCollection<HomePageCardLayout> HomePageCards { get; set; } = [
        new(new(HomePageCardSourceKind.Spotlight), 0, 0, 1, 2),
        new(new(HomePageCardSourceKind.UserRecommended), 0, 2, 1, 2),
        new(new(HomePageCardSourceKind.WorkRecommended), 0, 4, 1, 3)
        {
            SimpleWorkType = SimpleWorkType.IllustrationAndManga
        }
    ];

    [SettingsEntry(Symbol.Rename, DownloadPathMacroEntryHeader, DownloadPathMacroEntryDescription)]
    public string DownloadPathMacro { get; set; } = Path.Join(
        GetSpecialFolder(),
        "@{is_pic_set?[@{artist_name}] @{title}:}",
        "[@{artist_name}] @{id}@{is_pic_set?p@{pic_set_index}:}@{ext}"
    );
        

    [SettingsEntry(Symbol.TextPeriodAsterisk, WorkDownloadFormatEntryHeader, WorkDownloadFormatEntryDescription)]
    public string IllustrationDownloadFormat { get; set; } = Models.Download.IllustrationDownloadFormatToken.DefaultToken;

    public string UgoiraDownloadFormat { get; set; } = Models.Download.UgoiraDownloadFormatToken.DefaultToken;

    public string NovelDownloadFormat { get; set; } = Models.Download.NovelDownloadFormatToken.DefaultToken;

    [SettingsEntry(Symbol.ImageSplit, OverwriteDownloadedFileEntryHeader, OverwriteDownloadedFileEntryDescription)]
    public bool OverwriteDownloadedFile { get; set; }

    /// <summary>
    /// The max download tasks that are allowed to run concurrently
    /// </summary>
    [SettingsEntry(Symbol.DeveloperBoardLightning, MaxDownloadConcurrencyLevelEntryHeader, MaxDownloadConcurrencyLevelEntryDescription)]
    public int MaxDownloadTaskConcurrencyLevel { get; set; } = Environment.ProcessorCount / 4;

    [SettingsEntry(Symbol.Grid, SimpleWorkTypeEntryHeader, SimpleWorkTypeEntryDescription)]
    public SimpleWorkType DefaultSimpleWorkType { get; set; }

    [SettingsEntry(Symbol.ArrowTrending, RankOptionEntryHeader, RankOptionEntryDescription)]
    public RankOption IllustrationRankOption { get; set; }

    public RankOption NovelRankOption { get; set; }

    [SettingsEntry(Symbol.Key, ReverseSearchApiKeyEntryHeader, ReverseSearchApiKeyEntryDescription, ReverseSearchApiKeyEntryPlaceholder)]
    public string ReverseSearchApiKey { get; set; } = "";

    [SettingsEntry(Symbol.Cookies, WebCookieEntryHeader, WebCookieEntryDescription, WebCookieEntryPlaceholder)]
    public string WebCookie { get; set; } = "";

    [SettingsEntry(Symbol.TargetArrow, ReverseSearchResultSimilarityThresholdEntryHeader, ReverseSearchResultSimilarityThresholdEntryDescription)]
    public int ReverseSearchResultSimilarityThreshold { get; set; } = 80;

    /// <summary>
    /// The target filter that indicates the type of the client
    /// </summary>
    [SettingsEntry(Symbol.CodeBlock, TargetAPIPlatformEntryHeader, TargetAPIPlatformEntryDescription)]
    public TargetFilter TargetFilter { get; set; } = TargetFilter.ForAndroid;

    [SettingsEntry(Symbol.GlanceHorizontal, ThumbnailLayoutTypeEntryHeader, ThumbnailLayoutTypeEntryDescription)]
    public ThumbnailLayoutType ThumbnailLayoutType { get; set; } = ThumbnailLayoutType.LinedFlow;

    [SettingsEntry(Symbol.CardUiPortraitFlip, BrowseModeHeader, BrowseModeDescription)]
    public BrowseMode BrowseMode { get; set; } = BrowseMode.Swipe;

    [SettingsEntry(Symbol.ArrowBetweenDown, BrowseDirectionHeader, BrowseDirectionDescription)]
    public BrowseDirection BrowseDirection { get; set; } = BrowseDirection.LeftRight;

    [SettingsEntry(Symbol.SlideMultipleArrowRight, IllustrationViewerAutoPlayIntervalEntryHeader, IllustrationViewerAutoPlayIntervalEntryDescription)]
    public int IllustrationViewerAutoPlayInterval { get; set; } = 5;

    [SettingsEntry(Symbol.ArrowShuffle, IllustrationViewerAutoPlayModeEntryHeader, IllustrationViewerAutoPlayModeEntryDescription)]
    public IllustrationViewerAutoPlayMode IllustrationViewerAutoPlayMode { get; set; }

    [SettingsEntry(Symbol.ImageMultiple, IllustrationViewerAutoPlayScopeEntryHeader, IllustrationViewerAutoPlayScopeEntryDescription)]
    public IllustrationViewerAutoPlayScope IllustrationViewerAutoPlayScope { get; set; }

    [SettingsEntry(Symbol.TagDismiss, BlockedTagsEntryHeader, BlockedTagsEntryDescription, BlockedTagsEntryPlaceholder)]
    public ObservableCollection<string> BlockedTags { get; set; } = [];

    [SettingsEntry(Symbol.Router, ProxyTypeEntryHeader, ProxyTypeEntryDescription)]
    public ProxyType ProxyType { get; set; }

    [SettingsEntry(Symbol.Server, ProxyTextBoxEntryHeader, ProxyTextBoxEntryDescription)]
    public string Proxy { get; set; } = "";

    /// <summary>
    /// The mirror host for image server, Pixeval will do a simple substitution that
    /// changes the host of the original url(i.pximg.net) to this one.
    /// </summary>
    [SettingsEntry(Symbol.HardDrive, ImageMirrorServerEntryHeader, ImageMirrorServerEntryDescription, ImageMirrorServerEntryPlaceholder)]
    public string MirrorHost { get; set; } = "";

    [SettingsEntry(Symbol.Info, OpenWorkInfoByDefaultEntryHeader, OpenWorkInfoByDefaultEntryDescription)]
    public bool OpenWorkInfoByDefault { get; set; }

    public DateTime LastCheckedUpdate { get; set; } = DateTime.MinValue;

    [SettingsEntry(Symbol.Box, PixivNameResolverEntryHeader, PixivNameResolverEntryDescription, Placeholder = MakoHttpOptions.AppApiHost)]
    public ObservableCollection<string> PixivAppApiNameResolver { get; set; } =
    [
        "104.18.42.239",
        "172.64.145.17"
    ];

    [SettingsEntry(Placeholder = MakoHttpOptions.WebApiHost)]
    public ObservableCollection<string> PixivWebApiNameResolver { get; set; } =
    [
        "210.140.139.155",
        "210.140.139.156",
        "210.140.139.157"
    ];

    [SettingsEntry(Placeholder = MakoHttpOptions.AccountHost)]
    public ObservableCollection<string> PixivAccountNameResolver { get; set; } =
    [
        "210.140.139.155",
        "210.140.139.156",
        "210.140.139.157"
    ];

    [SettingsEntry(Placeholder = MakoHttpOptions.OAuthHost)]
    public ObservableCollection<string> PixivOAuthNameResolver { get; set; } =
    [
        "104.18.42.239",
        "172.64.145.17"
    ];

    [SettingsEntry(Placeholder = MakoHttpOptions.ImageHost)]
    public ObservableCollection<string> PixivImageNameResolver { get; set; } =
    [
        "210.140.139.134",
        "210.140.139.135",
        "210.140.139.136",
        "210.140.139.137"
    ];

    [SettingsEntry(Placeholder = MakoHttpOptions.ImageHost2)]
    public ObservableCollection<string> PixivImageNameResolver2 { get; set; } =
    [
        "210.140.139.135",
        "210.140.139.136",
        "210.140.139.137"
    ];

    public uint NovelFontColorInDarkMode { get; set; } = 0xFFFFFFFF;

    public uint NovelFontColorInLightMode { get; set; } = 0xFF000000;

    public uint NovelBackgroundInDarkMode { get; set; }

    public uint NovelBackgroundInLightMode { get; set; }

    [SettingsEntry(Symbol.LineThickness, NovelSettingsFontWeightEntryHeader, NovelSettingsFontWeightEntryDescription, NovelSettingsFontWeightEntryPlaceholder)]
    public FontWeight NovelFontWeight { get; set; } = FontWeight.Normal;

    [SettingsEntry(Symbol.TextFont, NovelSettingsFontFamilyEntryHeader, AppFontFamilyEntryDescription, AppFontFamilyEntryPlaceholder)]
    public ObservableCollection<string> NovelFontFamily { get; set; }

    [SettingsEntry(Symbol.TextFontSize, NovelSettingsFontSizeEntryHeader, NovelSettingsFontSizeEntryDescription)]
    public int NovelFontSize { get; set; } = 14;

    [SettingsEntry(Symbol.TextLineSpacing, NovelSettingsLineHeightEntryHeader, NovelSettingsLineHeightEntryDescription)]
    public int NovelLineHeight { get; set; } = 28;

    [SettingsEntry(Symbol.AutoFitWidth, NovelSettingsMaxWidthEntryHeader, NovelSettingsMaxWidthEntryDescription)]
    public int NovelMaxWidth { get; set; } = 1000;

    public double WindowWidth { get; set; } = 800;

    public double WindowHeight { get; set; } = 600;

    public bool IsMaximized { get; set; }

    public WorkType WorkType => DefaultSimpleWorkType is SimpleWorkType.IllustrationAndManga ? WorkType.Illustration : WorkType.Novel;

    public ApplicationTheme ActualTheme => Theme is ApplicationTheme.Default
        ? Application.Current!.ActualThemeVariant == ThemeVariant.Dark ? ApplicationTheme.Dark : ApplicationTheme.Light
        : Theme;

    [SettingsEntry(Symbol.ColorBackground, NovelSettingsBackgroundEntryHeader, NovelSettingsBackgroundEntryDescription)]
    public uint NovelBackground
    {
        get => ActualTheme is ApplicationTheme.Light ? NovelBackgroundInLightMode : NovelBackgroundInDarkMode;
        set
        {
            if (ActualTheme is ApplicationTheme.Light)
                NovelBackgroundInLightMode = value;
            else
                NovelBackgroundInDarkMode = value;
        }
    }

    [SettingsEntry(Symbol.TextColor, NovelSettingsFontColorEntryHeader, NovelSettingsFontColorEntryDescription)]
    public uint NovelFontColor
    {
        get => ActualTheme is ApplicationTheme.Light ? NovelFontColorInLightMode : NovelFontColorInDarkMode;
        set
        {
            if (ActualTheme is ApplicationTheme.Light)
                NovelFontColorInLightMode = value;
            else
                NovelFontColorInDarkMode = value;
        }
    }

    public Dictionary<string, Dictionary<string, JsonElement>> ExtensionSettings { get; set; } = [];

    public MakoConfiguration ToMakoConfiguration()
    {
        return new MakoConfiguration(EnableDomainFronting, DomainFrontingType, Proxy, WebCookie, MirrorHost, TargetFilter, 700, CultureInfo.CurrentCulture);
    }

    private static string GetSpecialFolder()
    {
        var picPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures, Environment.SpecialFolderOption.None);
        var docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.None);
        var picDirectory = Path.GetDirectoryName(picPath);
        return picDirectory == Path.GetDirectoryName(docPath)
            ? Path.Join(picDirectory!,
                $"@{{is_novel?{Path.GetFileName(docPath)}:{Path.GetFileName(picPath)}}}")
            : $"@{{is_novel?{docPath}:{picPath}}}";
    }
}

[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(LoginContext))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(HashSet<string>))]
[JsonSerializable(typeof(Dictionary<string, Dictionary<string, JsonElement>>))]
[JsonSerializable(typeof(ObservableCollection<string>))]
[JsonSerializable(typeof(ObservableCollection<HomePageCardLayout>))]
public partial class SettingsSerializerContext : JsonSerializerContext;
