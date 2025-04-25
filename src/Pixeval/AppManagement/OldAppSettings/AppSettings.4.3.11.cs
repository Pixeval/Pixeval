// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Pixeval.Attributes;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Mako.Global.Enum;
using Mako.Preference;
using Pixeval.Options;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Windows.Foundation;
using Windows.Globalization;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.AppManagement;

[GenerateConstructor(CallParameterlessConstructor = true), CopyTo]
// ReSharper disable once InconsistentNaming
public partial record AppSettings_4_3_11() : IWindowSettings
{
    public bool DownloadUpdateAutomatically { get; set; }

    public ElementTheme Theme { get; set; }

    public BackdropType Backdrop { get; set; } = MicaController.IsSupported() ? BackdropType.MicaAlt : DesktopAcrylicController.IsSupported() ? BackdropType.Acrylic : BackdropType.None;

    public bool EnableDomainFronting { get; set; } = true;

    public bool UseFileCache { get; set; } = true;

    public string AppFontFamilyName { get; set; } = AppSettingsResources.AppDefaultFontFamilyName;

    public MainPageTabItem DefaultSelectedTabItem { get; set; }

    public string DownloadPathMacro { get; set; } = GetSpecialFolder() + @"\@{if_pic_set=[@{artist_name}] @{title}}\[@{artist_name}] @{id}@{if_pic_set=p@{pic_set_index}}@{ext}";

    public IllustrationDownloadFormat IllustrationDownloadFormat { get; set; } = IllustrationDownloadFormat.Png;

    public UgoiraDownloadFormat UgoiraDownloadFormat { get; set; } = UgoiraDownloadFormat.WebPLossless;

    public NovelDownloadFormat NovelDownloadFormat { get; set; }

    public bool OverwriteDownloadedFile { get; set; }

    public int MaximumDownloadHistoryRecords { get; set; } = 100;

    public int MaxDownloadTaskConcurrencyLevel { get; set; } = Environment.ProcessorCount / 4;

    public bool DownloadWhenBookmarked { get; set; }

    public WorkSortOption WorkSortOption { get; set; }

    public SimpleWorkType SimpleWorkType { get; set; }

    public RankOption IllustrationRankOption { get; set; }

    public RankOption NovelRankOption { get; set; }

    public SearchIllustrationTagMatchOption SearchIllustrationTagMatchOption { get; set; } = SearchIllustrationTagMatchOption.PartialMatchForTags;

    public SearchNovelTagMatchOption SearchNovelTagMatchOption { get; set; } = SearchNovelTagMatchOption.PartialMatchForTags;

    public int MaximumSearchHistoryRecords { get; set; } = 50;

    public string ReverseSearchApiKey { get; set; } = "";

    public string WebCookie { get; set; } = "";

    public int ReverseSearchResultSimilarityThreshold { get; set; } = 80;

    public int MaximumSuggestionBoxSearchHistory { get; set; } = 10;

    public TargetFilter TargetFilter { get; set; } = TargetFilter.ForAndroid;

    public ThumbnailDirection ThumbnailDirection { get; set; } = ThumbnailDirection.Portrait;

    public ItemsViewLayoutType ItemsViewLayoutType { get; set; } = ItemsViewLayoutType.LinedFlow;

    public HashSet<string> BlockedTags { get; set; } = [];

    public ProxyType ProxyType { get; set; }

    public string Proxy { get; set; } = "";

    public string MirrorHost { get; set; } = "";

    public int MaximumBrowseHistoryRecords { get; set; } = 100;

    public bool UseSearchStartDate { get; set; }


    public bool UseSearchEndDate { get; set; }

    public DateTimeOffset SearchEndDate { get; set; } = DateTimeOffset.Now;

    public bool BrowseOriginalImage { get; set; }

    public DateTimeOffset LastCheckedUpdate { get; set; } = DateTimeOffset.MinValue;

    public bool ReconfirmationOfClosingWindow { get; set; } = true;

    public double ScrollRate { get; set; }

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

    public FontWeightsOption NovelFontWeight { get; set; } = FontWeightsOption.Normal;

    public string NovelFontFamily { get; set; } = AppSettingsResources.AppDefaultFontFamilyName;

    public int NovelFontSize { get; set; } = 14;

    public int NovelLineHeight { get; set; } = 28;

    public int NovelMaxWidth { get; set; } = 1000;

    public Size WindowSize { get; set; } = WindowHelper.EstimatedWindowSize().ToSize();

    public bool IsMaximized { get; set; }

    public WorkType WorkType => SimpleWorkType is SimpleWorkType.IllustAndManga ? WorkType.Illust : WorkType.Novel;

    public ElementTheme ActualTheme => Theme is ElementTheme.Default
        ? AppHelper.IsDarkMode ? ElementTheme.Dark : ElementTheme.Light
        : Theme;

    [AttributeIgnore(typeof(GenerateConstructorAttribute), typeof(AppContextAttribute<>))]
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

    public static CultureInfo CurrentCulture => ApplicationLanguages.PrimaryLanguageOverride.Let(language => CultureInfo.GetCultureInfo(string.IsNullOrWhiteSpace(language) ? AppSettingsResources.Bcl47 : language));

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
              @$"\@{{if_pic_all={Path.GetFileName(picPath)}}}@{{if_novel={Path.GetFileName(docPath)}}}"
            : $"@{{if_pic_all={picPath}}}@{{if_novel={docPath}}}";
    }
}
