// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

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

    [SettingsEntry(Symbol.Checkmark, DefaultSelectedTabEntryHeader, DefaultSelectedTabEntryDescription)]
    public MainPageTabItem DefaultSelectedTabItem { get; set; }

    [SettingsEntry(Symbol.Rename, DownloadPathMacroEntryHeader, DownloadPathMacroEntryDescription)]
    public string DownloadPathMacro { get; set; } = GetSpecialFolder() + @"\@{if_pic_set:[@{artist_name}] @{title}}\[@{artist_name}] @{id}@{if_pic_set:p@{pic_set_index}}@{ext}";

    [SettingsEntry(Symbol.TextPeriodAsterisk, WorkDownloadFormatEntryHeader, WorkDownloadFormatEntryDescription)]
    public IllustrationDownloadFormat IllustrationDownloadFormat { get; set; } = IllustrationDownloadFormat.Png;

    public UgoiraDownloadFormat UgoiraDownloadFormat { get; set; } = UgoiraDownloadFormat.WebPLossless;

    public NovelDownloadFormat NovelDownloadFormat { get; set; }

    [SettingsEntry(Symbol.FolderZip, LossyImageDownloadQualityEntryHeader, LossyImageDownloadQualityEntryDescription)]
    public int LossyImageDownloadQuality { get; set; } = -1;

    [SettingsEntry(Symbol.ImageSplit, OverwriteDownloadedFileEntryHeader, OverwriteDownloadedFileEntryDescription)]
    public bool OverwriteDownloadedFile { get; set; }

    [SettingsEntry(Symbol.History, MaximumDownloadHistoryRecordsEntryHeader, MaximumDownloadHistoryRecordsEntryDescription)]
    public int MaximumDownloadHistoryRecords { get; set; } = 100;

    /// <summary>
    /// The max download tasks that are allowed to run concurrently
    /// </summary>
    [SettingsEntry(Symbol.DeveloperBoardLightning, MaxDownloadConcurrencyLevelEntryHeader, MaxDownloadConcurrencyLevelEntryDescription)]
    public int MaxDownloadTaskConcurrencyLevel { get; set; } = Environment.ProcessorCount / 4;

    [SettingsEntry(Symbol.SaveEdit, DownloadWhenBookmarkedEntryHeader, DownloadWhenBookmarkedEntryDescription)]
    public bool DownloadWhenBookmarked { get; set; }

    /// <summary>
    /// The application-wide default sort option, any illustration page that supports
    /// different orders will use this as its default value
    /// </summary>
    [SettingsEntry(Symbol.ArrowSort, DefaultSearchSortOptionEntryHeader, DefaultSearchSortOptionEntryDescription)]
    public WorkSortOption WorkSortOption { get; set; }

    [SettingsEntry(Symbol.Grid, SimpleWorkTypeEntryHeader, SimpleWorkTypeEntryDescription)]
    public SimpleWorkType SimpleWorkType { get; set; }

    [SettingsEntry(Symbol.ArrowTrending, RankOptionEntryHeader, RankOptionEntryDescription)]
    public RankOption IllustrationRankOption { get; set; }

    public RankOption NovelRankOption { get; set; }

    /// <summary>
    /// The illustration tag match option for keyword search
    /// </summary>
    [SettingsEntry(Symbol.CheckmarkCircleSquare, DefaultSearchTagMatchOptionEntryHeader, DefaultSearchTagMatchOptionEntryDescription)]
    public SearchIllustrationTagMatchOption SearchIllustrationTagMatchOption { get; set; } = SearchIllustrationTagMatchOption.PartialMatchForTags;

    /// <summary>
    /// The novel tag match option for keyword search
    /// </summary>
    public SearchNovelTagMatchOption SearchNovelTagMatchOption { get; set; } = SearchNovelTagMatchOption.PartialMatchForTags;

    [SettingsEntry(Symbol.History, MaximumSearchHistoryRecordsEntryHeader, MaximumSearchHistoryRecordsEntryDescription)]
    public int MaximumSearchHistoryRecords { get; set; } = 50;

    [SettingsEntry(Symbol.Key, ReverseSearchApiKeyEntryHeader, ReverseSearchApiKeyEntryDescription, ReverseSearchApiKeyEntryPlaceholder)]
    public string ReverseSearchApiKey { get; set; } = "";

    [SettingsEntry(Symbol.Cookies, WebCookieEntryHeader, WebCookieEntryDescription, WebCookieEntryPlaceholder)]
    public string WebCookie { get; set; } = "";

    [SettingsEntry(Symbol.TargetArrow, ReverseSearchResultSimilarityThresholdEntryHeader, ReverseSearchResultSimilarityThresholdEntryDescription)]
    public int ReverseSearchResultSimilarityThreshold { get; set; } = 80;

    [SettingsEntry(Symbol.History, MaximumSuggestionBoxSearchHistoryEntryHeader, MaximumSuggestionBoxSearchHistoryEntryDescription)]
    public int MaximumSuggestionBoxSearchHistory { get; set; } = 10;

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

    [SettingsEntry(Symbol.History, MaximumBrowseHistoryRecordsEntryHeader, MaximumBrowseHistoryRecordsEntryDescription, MaximumBrowseHistoryRecordsEntryPlaceholder)]
    public int MaximumBrowseHistoryRecords { get; set; } = 100;

    [SettingsEntry(Symbol.ArrowCircleLeft, UseSearchStartDateEntryHeader, UseSearchStartDateEntryDescription)]
    public bool UseSearchStartDate { get; set; }

    public DateTime SearchStartDate { get; set; } = DateTime.Now - TimeSpan.FromDays(1);

    [SettingsEntry(Symbol.ArrowCircleRight, UseSearchEndDateEntryHeader, UseSearchEndDateEntryDescription)]
    public bool UseSearchEndDate { get; set; }

    public DateTime SearchEndDate { get; set; } = DateTime.UtcNow;

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

    public string TagsManagerWorkingPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures, Environment.SpecialFolderOption.Create);

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

    public WorkType WorkType => SimpleWorkType is SimpleWorkType.IllustrationAndManga ? WorkType.Illustration : WorkType.Novel;

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
        return new MakoConfiguration(EnableDomainFronting, DomainFrontingType, Proxy, WebCookie, MirrorHost, 700, CultureInfo.CurrentCulture);
    }

    private static string GetSpecialFolder()
    {
        var picPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures, Environment.SpecialFolderOption.None);
        var docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.None);
        var picDirectory = Path.GetDirectoryName(picPath);
        return picDirectory == Path.GetDirectoryName(docPath)
            ? picDirectory +
              @$"\@{{if_pic_all:{Path.GetFileName(picPath)}}}@{{if_novel:{Path.GetFileName(docPath)}}}"
            : $"@{{if_pic_all:{picPath}}}@{{if_novel:{docPath}}}";
    }
}

[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(LoginContext))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(HashSet<string>))]
[JsonSerializable(typeof(Dictionary<string, Dictionary<string, JsonElement>>))]
// MultiStringsAppSettingsEntry 使用 ObservableCollection<string>
[JsonSerializable(typeof(ObservableCollection<string>))]
public partial class SettingsSerializerContext : JsonSerializerContext;
