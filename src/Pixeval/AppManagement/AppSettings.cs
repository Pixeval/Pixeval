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
using Windows.Foundation;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Attributes;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Preference;
using Pixeval.Options;
using Pixeval.Util.UI;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;
using WinUI3Utilities.Controls;

namespace Pixeval.AppManagement;

[GenerateConstructor, Reset]
public partial record AppSettings : IWindowSettings
{
    public AppSettings()
    {
    }

    [SettingsEntry(IconGlyph.CommunicationsE95A, nameof(SettingsPageResources.DownloadUpdateAutomaticallyEntryHeader), nameof(SettingsPageResources.DownloadUpdateAutomaticallyEntryDescription))]
    public bool DownloadUpdateAutomatically { get; set; }

    /// <summary>
    /// The Application Theme
    /// </summary>
    [SettingsEntry(IconGlyph.PersonalizeE771, nameof(SettingsPageResources.ThemeEntryHeader), nameof(SettingsPageResources.ThemeEntryDescriptionHyperlinkButtonContent))]
    public ElementTheme Theme { get; set; }

    [SettingsEntry(IconGlyph.ColorE790, nameof(SettingsPageResources.BackdropEntryHeader), null)]
    public BackdropType Backdrop { get; set; } = MicaController.IsSupported() ? BackdropType.MicaAlt : DesktopAcrylicController.IsSupported() ? BackdropType.Acrylic : BackdropType.None;

    [SettingsEntry(IconGlyph.NetworkE968, nameof(SettingsPageResources.EnableDomainFrontingEntryHeader), nameof(SettingsPageResources.EnableDomainFrontingEntryDescription))]
    public bool EnableDomainFronting { get; set; } = true;

    /// <summary>
    /// Indicates whether a <see cref="TeachingTip" /> should be displayed
    /// when user clicks "Generate Link"
    /// </summary>
    [SettingsEntry(IconGlyph.LinkE71B, nameof(SettingsPageResources.GenerateHelpLinkEntryHeader), nameof(SettingsPageResources.GenerateHelpLinkEntryDescription))]
    public bool DisplayTeachingTipWhenGeneratingAppLink { get; set; } = true;

    [SettingsEntry(IconGlyph.FileExplorerEC50, nameof(SettingsPageResources.UseFileCacheEntryHeader), nameof(SettingsPageResources.UseFileCacheEntryDescription))]
    public bool UseFileCache { get; set; }

    [SettingsEntry(IconGlyph.FontE8D2, nameof(SettingsPageResources.AppFontFamilyEntryHeader), nameof(SettingsPageResources.OpenFontSettingsHyperlinkButtonContent))]
    public string AppFontFamilyName { get; set; } = AppSettingsResources.AppDefaultFontFamilyName;

    [SettingsEntry(IconGlyph.CheckMarkE73E, nameof(SettingsPageResources.DefaultSelectedTabEntryHeader), nameof(SettingsPageResources.DefaultSelectedTabEntryDescription))]
    public MainPageTabItem DefaultSelectedTabItem { get; set; }

    [SettingsEntry(IconGlyph.RenameE8AC, nameof(SettingsPageResources.DefaultDownloadPathMacroEntryHeader), nameof(SettingsPageResources.DefaultDownloadPathMacroEntryDescriptionContent))]
    public string DefaultDownloadPathMacro { get; set; } = GetSpecialFolder() + @"\@{if_manga=[@{artist_name}] @{title}}\[@{artist_name}] @{id}@{if_manga=p@{manga_index}}@{ext}";

    public UgoiraDownloadFormat UgoiraDownloadFormat { get; set; } = UgoiraDownloadFormat.WebPLossless;

    public IllustrationDownloadFormat IllustrationDownloadFormat { get; set; } = IllustrationDownloadFormat.Png;

    public NovelDownloadFormat NovelDownloadFormat { get; set; }

    [SettingsEntry(IconGlyph.ScanE8FE, nameof(SettingsPageResources.OverwriteDownloadedFileEntryHeader), nameof(SettingsPageResources.OverwriteDownloadedFileEntryDescription))]
    public bool OverwriteDownloadedFile { get; set; }

    [SettingsEntry(IconGlyph.HistoryE81C, nameof(SettingsPageResources.MaximumDownloadHistoryRecordsEntryHeader), nameof(SettingsPageResources.MaximumDownloadHistoryRecordsEntryDescription))]
    public int MaximumDownloadHistoryRecords { get; set; } = 100;

    /// <summary>
    /// The max download tasks that are allowed to run concurrently
    /// </summary>
    [SettingsEntry(IconGlyph.LightningBoltE945, nameof(SettingsPageResources.MaxDownloadConcurrencyLevelEntryHeader), nameof(SettingsPageResources.MaxDownloadConcurrencyLevelEntryDescription))]
    public int MaxDownloadTaskConcurrencyLevel { get; set; } = Environment.ProcessorCount / 2;

    [SettingsEntry(IconGlyph.SaveLocalE78C, nameof(SettingsPageResources.DownloadWhenBookmarkedEntryHeader), nameof(SettingsPageResources.DownloadWhenBookmarkedEntryDescription))]
    public bool DownloadWhenBookmarked { get; set; }

    /// <summary>
    /// The application-wide default sort option, any illustration page that supports
    /// different orders will use this as its default value
    /// </summary>
    [SettingsEntry(IconGlyph.SortE8CB, nameof(SettingsPageResources.DefaultSearchSortOptionEntryHeader), nameof(SettingsPageResources.DefaultSearchSortOptionEntryDescription))]
    public WorkSortOption WorkSortOption { get; set; }

    [SettingsEntry(IconGlyph.ViewAllE8A9, nameof(SettingsPageResources.DefaultSearchTagMatchOptionEntryHeader), nameof(SettingsPageResources.DefaultSearchTagMatchOptionEntryDescription))]
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

    [SettingsEntry(IconGlyph.HistoryE81C, nameof(SettingsPageResources.MaximumSearchHistoryRecordsEntryHeader), nameof(SettingsPageResources.MaximumSearchHistoryRecordsEntryDescription))]
    public int MaximumSearchHistoryRecords { get; set; } = 50;

    [SettingsEntry(IconGlyph.EaseOfAccessE776, nameof(SettingsPageResources.SearchDurationEntryHeader), nameof(SettingsPageResources.SearchDurationEntryDescription))]
    public SearchDuration SearchDuration { get; set; } = SearchDuration.Undecided;

    [SettingsEntry(IconGlyph.StopwatchE916, nameof(SettingsPageResources.UsePreciseRangeForSearchEntryHeader), nameof(SettingsPageResources.UsePreciseRangeForSearchEntryDescription))]
    public bool UsePreciseRangeForSearch { get; set; }

    [SettingsEntry(IconGlyph.SearchAndAppsE773, nameof(SettingsPageResources.ReverseSearchApiKeyEntryHeader), nameof(SettingsPageResources.ReverseSearchApiKeyEntryDescriptionHyperlinkButtonContent))]
    public string? ReverseSearchApiKey { get; set; }

    [SettingsEntry(IconGlyph.FilterE71C, nameof(SettingsPageResources.ReverseSearchResultSimilarityThresholdEntryHeader), nameof(SettingsPageResources.ReverseSearchResultSimilarityThresholdEntryDescription))]
    public int ReverseSearchResultSimilarityThreshold { get; set; } = 80;

    [SettingsEntry(IconGlyph.SetHistoryStatus2F739, nameof(SettingsPageResources.MaximumSuggestionBoxSearchHistoryEntryHeader), nameof(SettingsPageResources.MaximumSuggestionBoxSearchHistoryEntryDescription))]
    public int MaximumSuggestionBoxSearchHistory { get; set; } = 10;

    /// <summary>
    /// The target filter that indicates the type of the client
    /// </summary>
    [SettingsEntry(IconGlyph.CommandPromptE756, nameof(SettingsPageResources.TargetAPIPlatformEntryHeader), nameof(SettingsPageResources.TargetAPIPlatformEntryDescription))]
    public TargetFilter TargetFilter { get; set; } = TargetFilter.ForAndroid;

    [SettingsEntry(IconGlyph.TypeE97C, nameof(SettingsPageResources.ThumbnailDirectionEntryHeader), nameof(SettingsPageResources.ThumbnailDirectionEntryDescription))]
    public ThumbnailDirection ThumbnailDirection { get; set; } = ThumbnailDirection.Portrait;

    [SettingsEntry(IconGlyph.TilesECA5, nameof(SettingsPageResources.ItemsViewLayoutTypeEntryHeader), nameof(SettingsPageResources.ItemsViewLayoutTypeEntryDescription))]
    public ItemsViewLayoutType ItemsViewLayoutType { get; set; } = ItemsViewLayoutType.LinedFlow;

    [SettingsEntry(IconGlyph.Blocked2ECE4, nameof(SettingsPageResources.BlockedTagsEntryHeader), nameof(SettingsPageResources.BlockedTagsEntryDescription))]
    public HashSet<string> BlockedTags { get; set; } = [];

    /// <summary>
    /// The mirror host for image server, Pixeval will do a simple substitution that
    /// changes the host of the original url(i.pximg.net) to this one.
    /// </summary>
    [SettingsEntry(IconGlyph.HardDriveEDA2, nameof(SettingsPageResources.ImageMirrorServerEntryHeader), nameof(SettingsPageResources.ImageMirrorServerEntryDescription))]
    public string? MirrorHost { get; set; } = null;

    [SettingsEntry(IconGlyph.HistoryE81C, nameof(SettingsPageResources.MaximumBrowseHistoryRecordsEntryHeader), nameof(SettingsPageResources.MaximumBrowseHistoryRecordsEntryDescription))]
    public int MaximumBrowseHistoryRecords { get; set; } = 100;

    public DateTimeOffset SearchStartDate { get; set; } = DateTimeOffset.Now - TimeSpan.FromDays(1);

    public DateTimeOffset SearchEndDate { get; set; } = DateTimeOffset.Now;

    [SettingsEntry(IconGlyph.FitPageE9A6, nameof(SettingsPageResources.BrowserOriginalImageEntryHeader), nameof(SettingsPageResources.BrowserOriginalImageEntryDescription))]
    public bool BrowserOriginalImage { get; set; }

    [AttributeIgnore(typeof(ResetAttribute))]
    public DateTimeOffset LastCheckedUpdate { get; set; } = DateTimeOffset.MinValue;

    public string[] PixivWebApiNameResolver { get; set; } =
    [
        "210.140.131.219",
        "210.140.131.223",
        "210.140.131.226"
    ];

    public string[] PixivAccountNameResolver { get; set; } =
    [
        "210.140.131.219",
        "210.140.131.223",
        "210.140.131.226"
    ];

    public string[] PixivOAuthNameResolver { get; set; } =
    [
        "210.140.131.219",
        "210.140.131.223",
        "210.140.131.226"
    ];

    public string[] PixivAppApiNameResolver { get; set; } =
    [
        "210.140.131.199",
        "210.140.131.219",
        "210.140.131.223",
        "210.140.131.226"
    ];

    public string[] PixivImageNameResolver { get; set; } =
    [
        "210.140.92.144",
        "210.140.92.141",
        "210.140.92.142",
        "210.140.92.143"
    ];

    public string[] PixivImageNameResolver2 { get; set; } =
    [
        "210.140.92.143",
        "210.140.92.141",
        "210.140.92.142"
    ];

    public string TagsManagerWorkingPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures, Environment.SpecialFolderOption.Create);

    public uint NovelFontColorInDarkMode { get; set; } = 0xFFFFFFFF;

    public uint NovelFontColorInLightMode { get; set; } = 0xFF000000;

    public uint NovelBackgroundInDarkMode { get; set; } = 0;

    public uint NovelBackgroundInLightMode { get; set; } = 0;

    public FontWeightsOption NovelFontWeight { get; set; } = FontWeightsOption.Normal;

    public string NovelFontFamily { get; set; } = AppSettingsResources.AppDefaultFontFamilyName;

    public double NovelFontSize { get; set; } = 14;

    public double NovelLineHeight { get; set; } = 28;

    public double NovelMaxWidth { get; set; } = 1000;

    public Size WindowSize { get; set; } = WindowHelper.EstimatedWindowSize().ToSize();

    public bool IsMaximized { get; set; }

    [AttributeIgnore(typeof(GenerateConstructorAttribute), typeof(AppContextAttribute<>))]
    public WorkType WorkType => SimpleWorkType is SimpleWorkType.IllustAndManga ? WorkType.Illust : WorkType.Novel;

    public MakoClientConfiguration ToMakoClientConfiguration()
    {
        return new MakoClientConfiguration(5000, EnableDomainFronting, MirrorHost, CultureInfo.CurrentUICulture);
    }

    private static string GetSpecialFolder()
    {
        var picPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures, Environment.SpecialFolderOption.Create);
        var docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.Create);
        var picDirectory = Path.GetDirectoryName(picPath);
        return picDirectory == Path.GetDirectoryName(docPath)
            ? picDirectory +
              @$"\@{{if_illust={Path.GetFileName(picPath)}}}@{{if_novel={Path.GetFileName(docPath)}}}"
            : @$"\@{{if_illust={picPath}}}@{{if_novel={docPath}}}";
    }
}
