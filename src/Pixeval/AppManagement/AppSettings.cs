#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/AppSetting.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json.Serialization;
using Windows.Foundation;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Pixeval.Attributes;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Preference;
using Pixeval.Options;
using Pixeval.Util.UI;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;
using FluentIcons.Common;
using Microsoft.Windows.Globalization;
using Pixeval.Upscaling;
using Pixeval.Utilities;
using static Pixeval.SettingsPageResources;

namespace Pixeval.AppManagement;

[GenerateConstructor(CallParameterlessConstructor = true), Reset]
public partial record AppSettings() : IWindowSettings
{
    [SettingsEntry(Symbol.Communication, nameof(DownloadUpdateAutomaticallyEntryHeader), nameof(DownloadUpdateAutomaticallyEntryDescription))]
    public bool DownloadUpdateAutomatically { get; set; }

    /// <summary>
    /// The Application Theme
    /// </summary>
    [SettingsEntry(Symbol.DarkTheme, nameof(ThemeEntryHeader), nameof(ThemeEntryDescriptionHyperlinkButtonContent))]
    public ElementTheme Theme { get; set; }

    [SettingsEntry(Symbol.PaintBrush, nameof(BackdropEntryHeader), null)]
    public BackdropType Backdrop { get; set; } = MicaController.IsSupported() ? BackdropType.MicaAlt : DesktopAcrylicController.IsSupported() ? BackdropType.Acrylic : BackdropType.None;

    [SettingsEntry(Symbol.ShieldTask, nameof(EnableDomainFrontingEntryHeader), nameof(EnableDomainFrontingEntryDescription))]
    public bool EnableDomainFronting { get; set; } = true;

    [SettingsEntry(Symbol.Database, nameof(UseFileCacheEntryHeader), nameof(UseFileCacheEntryDescription))]
    public bool UseFileCache { get; set; }

    [SettingsEntry(Symbol.TextFont, nameof(AppFontFamilyEntryHeader), nameof(OpenFontSettingsHyperlinkButtonContent))]
    public string AppFontFamilyName { get; set; } = AppSettingsResources.AppDefaultFontFamilyName;

    [SettingsEntry(Symbol.Checkmark, nameof(DefaultSelectedTabEntryHeader), nameof(DefaultSelectedTabEntryDescription))]
    public MainPageTabItem DefaultSelectedTabItem { get; set; }

    [SettingsEntry(Symbol.Rename, nameof(DownloadPathMacroEntryHeader), nameof(DownloadPathMacroEntryDescription))]
    public string DownloadPathMacro { get; set; } = GetSpecialFolder() + @"\@{if_manga=[@{artist_name}] @{title}}\[@{artist_name}] @{id}@{if_manga=p@{manga_index}}@{ext}";

    public UgoiraDownloadFormat UgoiraDownloadFormat { get; set; } = UgoiraDownloadFormat.WebPLossless;

    public IllustrationDownloadFormat IllustrationDownloadFormat { get; set; } = IllustrationDownloadFormat.Png;

    public NovelDownloadFormat NovelDownloadFormat { get; set; }

    [SettingsEntry(Symbol.ImageSplit, nameof(OverwriteDownloadedFileEntryHeader), nameof(OverwriteDownloadedFileEntryDescription))]
    public bool OverwriteDownloadedFile { get; set; }

    [SettingsEntry(Symbol.History, nameof(MaximumDownloadHistoryRecordsEntryHeader), nameof(MaximumDownloadHistoryRecordsEntryDescription))]
    public int MaximumDownloadHistoryRecords { get; set; } = 100;

    /// <summary>
    /// The max download tasks that are allowed to run concurrently
    /// </summary>
    [SettingsEntry(Symbol.DeveloperBoardLightning, nameof(MaxDownloadConcurrencyLevelEntryHeader), nameof(MaxDownloadConcurrencyLevelEntryDescription))]
    public int MaxDownloadTaskConcurrencyLevel { get; set; } = Environment.ProcessorCount / 4;

    [SettingsEntry(Symbol.SaveEdit, nameof(DownloadWhenBookmarkedEntryHeader), nameof(DownloadWhenBookmarkedEntryDescription))]
    public bool DownloadWhenBookmarked { get; set; }

    /// <summary>
    /// The application-wide default sort option, any illustration page that supports
    /// different orders will use this as its default value
    /// </summary>
    [SettingsEntry(Symbol.ArrowSort, nameof(DefaultSearchSortOptionEntryHeader), nameof(DefaultSearchSortOptionEntryDescription))]
    public WorkSortOption WorkSortOption { get; set; }

    [SettingsEntry(Symbol.Grid, nameof(DefaultSearchTagMatchOptionEntryHeader), nameof(DefaultSearchTagMatchOptionEntryDescription))]
    public SimpleWorkType SimpleWorkType { get; set; }

    public RankOption IllustrationRankOption { get; set; }

    public RankOption NovelRankOption { get; set; }

    /// <summary>
    /// The illustration tag match option for keyword search
    /// </summary>
    public SearchIllustrationTagMatchOption SearchIllustrationTagMatchOption { get; set; } = SearchIllustrationTagMatchOption.PartialMatchForTags;

    /// <summary>
    /// The novel tag match option for keyword search
    /// </summary>
    public SearchNovelTagMatchOption SearchNovelTagMatchOption { get; set; } = SearchNovelTagMatchOption.PartialMatchForTags;

    [SettingsEntry(Symbol.History, nameof(MaximumSearchHistoryRecordsEntryHeader), nameof(MaximumSearchHistoryRecordsEntryDescription))]
    public int MaximumSearchHistoryRecords { get; set; } = 50;

    [SettingsEntry(Symbol.Clock, nameof(SearchDurationEntryHeader), nameof(SearchDurationEntryDescription))]
    public SearchDuration SearchDuration { get; set; } = SearchDuration.Undecided;

    [SettingsEntry(Symbol.Timer, nameof(UsePreciseRangeForSearchEntryHeader), nameof(UsePreciseRangeForSearchEntryDescription))]
    public bool UsePreciseRangeForSearch { get; set; }

    [SettingsEntry(Symbol.Key, nameof(ReverseSearchApiKeyEntryHeader), nameof(ReverseSearchApiKeyEntryDescriptionHyperlinkButtonContent))]
    public string ReverseSearchApiKey { get; set; } = "";

    [SettingsEntry(Symbol.Cookies, nameof(WebCookieEntryHeader), nameof(WebCookieEntryDescription))]
    public string WebCookie { get; set; } = "";

    [SettingsEntry(Symbol.TargetArrow, nameof(ReverseSearchResultSimilarityThresholdEntryHeader), nameof(ReverseSearchResultSimilarityThresholdEntryDescription))]
    public int ReverseSearchResultSimilarityThreshold { get; set; } = 80;

    [SettingsEntry(Symbol.History, nameof(MaximumSuggestionBoxSearchHistoryEntryHeader), nameof(MaximumSuggestionBoxSearchHistoryEntryDescription))]
    public int MaximumSuggestionBoxSearchHistory { get; set; } = 10;
    
    /// <summary>
    /// The target filter that indicates the type of the client
    /// </summary>
    [SettingsEntry(Symbol.CodeBlock, nameof(TargetAPIPlatformEntryHeader), nameof(TargetAPIPlatformEntryDescription))]
    public TargetFilter TargetFilter { get; set; } = TargetFilter.ForAndroid;

    [SettingsEntry(Symbol.Orientation, nameof(ThumbnailDirectionEntryHeader), nameof(ThumbnailDirectionEntryDescription))]
    public ThumbnailDirection ThumbnailDirection { get; set; } = ThumbnailDirection.Portrait;

    [SettingsEntry(Symbol.GlanceHorizontal, nameof(ItemsViewLayoutTypeEntryHeader), nameof(ItemsViewLayoutTypeEntryDescription))]
    public ItemsViewLayoutType ItemsViewLayoutType { get; set; } = ItemsViewLayoutType.LinedFlow;

    [SettingsEntry(Symbol.TagDismiss, nameof(BlockedTagsEntryHeader), nameof(BlockedTagsEntryDescription))]
    public HashSet<string> BlockedTags { get; set; } = [];

    [SettingsEntry(Symbol.Router, nameof(ProxyTypeEntryHeader), nameof(ProxyTypeEntryDescription))]
    public ProxyType ProxyType { get; set; }

    [SettingsEntry(Symbol.Server, nameof(ProxyTextBoxEntryHeader), nameof(ProxyTextBoxEntryDescription))]
    public string Proxy { get; set; } = "";

    /// <summary>
    /// The mirror host for image server, Pixeval will do a simple substitution that
    /// changes the host of the original url(i.pximg.net) to this one.
    /// </summary>
    [SettingsEntry(Symbol.HardDrive, nameof(ImageMirrorServerEntryHeader), nameof(ImageMirrorServerEntryDescription))]
    public string MirrorHost { get; set; } = "";

    [SettingsEntry(Symbol.History, nameof(MaximumBrowseHistoryRecordsEntryHeader), nameof(MaximumBrowseHistoryRecordsEntryDescription))]
    public int MaximumBrowseHistoryRecords { get; set; } = 100;

    public DateTimeOffset SearchStartDate { get; set; } = DateTimeOffset.Now - TimeSpan.FromDays(1);

    public DateTimeOffset SearchEndDate { get; set; } = DateTimeOffset.Now;

    [SettingsEntry(Symbol.ImageSparkle, nameof(BrowseOriginalImageEntryHeader), nameof(BrowseOriginalImageEntryDescription))]
    public bool BrowseOriginalImage { get; set; }

    [AttributeIgnore(typeof(ResetAttribute))]
    public DateTimeOffset LastCheckedUpdate { get; set; } = DateTimeOffset.MinValue;

    [SettingsEntry(Symbol.EyeTracking, nameof(UpscalerModelEntryHeader), nameof(UpscalerModelEntryDescription))]
    public RealESRGANModel UpscalerModel { get; set; } = RealESRGANModel.RealESRGANX4Plus;

    [SettingsEntry(Symbol.RatioOneToOne, nameof(UpscalerScaleRatioEntryHeader), nameof(UpscalerScaleRatioEntryDescription))]
    public int UpscalerScaleRatio { get; set; } = 4;

    [SettingsEntry(Symbol.ImageGlobe, nameof(UpscalerOutputTypeEntryHeader), nameof(UpscalerOutputTypeEntryDescription))]
    public UpscalerOutputType UpscalerOutputType { get; set; } = UpscalerOutputType.Png;

    public bool ShowUpscalerTeachingTip { get; set; } = true;

    [SettingsEntry(Symbol.Box, nameof(PixivNameResolverHeaderText), nameof(PixivNameResolverDescriptionText))]
    public string[] PixivAppApiNameResolver { get; set; } =
    [
        "210.140.139.155",
        "210.140.139.156",
        "210.140.139.157",
        "210.140.139.158"
    ];

    public string[] PixivWebApiNameResolver { get; set; } =
    [
        "210.140.139.155",
        "210.140.139.156",
        "210.140.139.157"
    ];

    public string[] PixivAccountNameResolver { get; set; } =
    [
        "210.140.139.155",
        "210.140.139.156",
        "210.140.139.157"
    ];

    public string[] PixivOAuthNameResolver { get; set; } =
    [
        "210.140.139.155",
        "210.140.139.156",
        "210.140.139.157"
    ];

    public string[] PixivImageNameResolver { get; set; } =
    [
        "210.140.139.134",
        "210.140.139.135",
        "210.140.139.136",
        "210.140.139.137"
    ];

    public string[] PixivImageNameResolver2 { get; set; } =
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

    [SettingsEntry(Symbol.LineThickness, nameof(NovelSettingsFontWeightEntryHeader), nameof(NovelSettingsFontWeightEntryDescription))]
    public FontWeightsOption NovelFontWeight { get; set; } = FontWeightsOption.Normal;

    [SettingsEntry(Symbol.TextFont, nameof(NovelSettingsFontFamilyEntryHeader), nameof(OpenFontSettingsHyperlinkButtonContent))]
    public string NovelFontFamily { get; set; } = AppSettingsResources.AppDefaultFontFamilyName;

    [SettingsEntry(Symbol.TextFontSize, nameof(NovelSettingsFontSizeEntryHeader), nameof(NovelSettingsFontSizeEntryDescription))]
    public int NovelFontSize { get; set; } = 14;

    [SettingsEntry(Symbol.TextLineSpacing, nameof(NovelSettingsLineHeightEntryHeader), nameof(NovelSettingsLineHeightEntryDescription))]
    public int NovelLineHeight { get; set; } = 28;

    [SettingsEntry(Symbol.AutoFitWidth, nameof(NovelSettingsMaxWidthEntryHeader), nameof(NovelSettingsMaxWidthEntryDescription))]
    public int NovelMaxWidth { get; set; } = 1000;

    public Size WindowSize { get; set; } = WindowHelper.EstimatedWindowSize().ToSize();

    public bool IsMaximized { get; set; }

    public WorkType WorkType => SimpleWorkType is SimpleWorkType.IllustAndManga ? WorkType.Illust : WorkType.Novel;

    public ElementTheme ActualTheme => Theme is ElementTheme.Default
        ? AppHelper.IsDarkMode ? ElementTheme.Dark : ElementTheme.Light
        : Theme;

    [AttributeIgnore(typeof(GenerateConstructorAttribute), typeof(AppContextAttribute<>))]
    [SettingsEntry(Symbol.ColorBackground, nameof(NovelSettingsBackgroundEntryHeader), nameof(NovelSettingsBackgroundEntryDescription))]
    public uint NovelBackground
    {
        get => ActualTheme is ElementTheme.Light ? NovelBackgroundInLightMode : NovelBackgroundInDarkMode;
        set
        {
            if (ActualTheme is ElementTheme.Light)
                NovelBackgroundInLightMode = value;
            else
                NovelBackgroundInDarkMode = value;
        }
    }

    [AttributeIgnore(typeof(GenerateConstructorAttribute), typeof(AppContextAttribute<>))]
    [SettingsEntry(Symbol.TextColor, nameof(NovelSettingsFontColorEntryHeader), nameof(NovelSettingsFontColorEntryDescription))]
    public uint NovelFontColor
    {
        get => ActualTheme is ElementTheme.Light ? NovelFontColorInLightMode : NovelFontColorInDarkMode;
        set
        {
            if (ActualTheme is ElementTheme.Light)
                NovelFontColorInLightMode = value;
            else
                NovelFontColorInDarkMode = value;
        }
    }

    public static CultureInfo CurrentCulture => ApplicationLanguages.PrimaryLanguageOverride.Let(language => string.IsNullOrEmpty(language) ? CultureInfo.CurrentUICulture : CultureInfo.GetCultureInfo(language));

    public MakoClientConfiguration ToMakoClientConfiguration()
    {
        return new MakoClientConfiguration(5000, EnableDomainFronting, Proxy, WebCookie, MirrorHost, CurrentCulture);
    }

    private static string GetSpecialFolder()
    {
        var picPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures, Environment.SpecialFolderOption.Create);
        var docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.Create);
        var picDirectory = Path.GetDirectoryName(picPath);
        return picDirectory == Path.GetDirectoryName(docPath)
            ? picDirectory +
              @$"\@{{if_illust={Path.GetFileName(picPath)}}}@{{if_novel={Path.GetFileName(docPath)}}}"
            : $"@{{if_illust={picPath}}}@{{if_novel={docPath}}}";
    }
}

[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(HashSet<string>))]
public partial class SettingsSerializeContext : JsonSerializerContext;
