using System;
using System.IO;
using AutoSettingsPage;
using AutoSettingsPage.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.AppManagement;
using Pixeval.AppManagement.Settings;
using Pixeval.I18N;
using Pixeval.Models.Options;
using Pixeval.Models.Settings;
using Pixeval.Utilities;
using SharpYaml;

namespace Pixeval.Tests;

[TestClass]
[DoNotParallelize]
public sealed class NestedSettingsTest
{
    [ClassInitialize]
    public static void Initialize(TestContext _)
    {
        var projectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Pixeval"));
        if (Directory.Exists(projectPath))
            I18NManager.CandidatePaths.Add(projectPath);
        I18NManager.Register(new JsonMarkdownLangPlugin(), LanguageHelper.DefaultLanguage);
        LocalSettingsEntryHelper.Initialize();
    }

    [TestMethod]
    public void SwitchAndChildrenShouldUpdateAndResetTheSameSubSettings()
    {
        var settings = new ApplicationSettingsGroup();
        var changes = 0;
        var entries = SettingsBuilder.CreateGroup(settings)
            .MultiValuesWithSwitch(t => t.FileCache, t => t.LimitFileCacheSize,
                children => children.Int(t => t.FileCacheSizeLimitInMegabytes, 1, 10000, 1),
                entry => entry.MainValue.ValueChanged += _ => ++changes)
            .Build();
        var expander = (MultiValuesWithMainValueEntry<ApplicationSettingsGroup, FileCacheSettings, BoolSettingsEntry<FileCacheSettings>>) entries[0];
        var size = (ISingleValueSettingsEntry<int>) expander.Entries[0];

        expander.MainValue.Value = true;
        size.Value = 1234;

        Assert.AreSame(settings.FileCache, expander.Settings);
        Assert.IsTrue(settings.FileCache.LimitFileCacheSize);
        Assert.AreEqual(1234, settings.FileCache.FileCacheSizeLimitInMegabytes);
        Assert.AreEqual(nameof(ApplicationSettingsGroup.FileCache), expander.Token);
        expander.LocalValueReset(new AppSettings());
        Assert.IsFalse(settings.FileCache.LimitFileCacheSize);
        Assert.AreEqual(2048, settings.FileCache.FileCacheSizeLimitInMegabytes);
        Assert.AreEqual(2, changes);

        size.Value = 512;
        expander.Entries[0].LocalValueReset(new AppSettings());
        Assert.AreEqual(2048, size.Value);
    }

    [TestMethod]
    public void EnumMainValueShouldUseTheSubSettingsAndReset()
    {
        var settings = new BrowsingExperienceSettingsGroup();
        var entries = SettingsBuilder.CreateGroup(settings)
            .MultiValuesWithMainValue(t => t.ThumbnailLayout, t => t.ThumbnailLayoutType,
                children => children.Int(t => t.IllustrationGridItemSize, 50, 1000, 10))
            .Build();
        var expander = (IMultiValuesWithMainValueSettingsEntry<ISingleValueSettingsEntry<object>>) entries[0];
        var mainValue = expander.MainValue;
        var alternative = ThumbnailLayoutType.Grid;
        mainValue.Value = alternative;
        ((ISingleValueSettingsEntry<int>) expander.Entries[0]).Value = 300;

        Assert.AreEqual(alternative, settings.ThumbnailLayout.ThumbnailLayoutType);
        Assert.AreEqual(300, settings.ThumbnailLayout.IllustrationGridItemSize);
        entries[0].LocalValueReset(new AppSettings());
        Assert.AreEqual(ThumbnailLayoutType.LinedFlow, settings.ThumbnailLayout.ThumbnailLayoutType);
        Assert.AreEqual(150, settings.ThumbnailLayout.IllustrationGridItemSize);
    }

    [TestMethod]
    public void MultiValuesShouldResetChildrenFromTheSuppliedNestedSettings()
    {
        var settings = new SearchSettingsGroup();
        var entries = SettingsBuilder.CreateGroup(settings)
            .MultiValues(t => t.RankOptions, children => children
                .SingleValue(t => t.IllustrationRankOption)
                .SingleValue(t => t.NovelRankOption))
            .Build();
        var imported = new AppSettings();
        imported.SearchSettings.RankOptions.IllustrationRankOption = Mako.Global.Enum.RankOption.Month;
        imported.SearchSettings.RankOptions.NovelRankOption = Mako.Global.Enum.RankOption.Week;

        entries[0].LocalValueReset(imported);

        Assert.AreEqual(imported.SearchSettings.RankOptions, settings.RankOptions);
        Assert.AreEqual(nameof(SearchSettingsGroup.RankOptions), entries[0].Token);
    }

    [TestMethod]
    public void ProxyChangesShouldUseTheNestedProxySettings()
    {
        var settings = new NetworkSettingsGroup();
        string? proxy = null;
        var entries = SettingsBuilder.CreateGroup(settings)
            .Proxy(entry => entry.ProxyChanged += value => proxy = value)
            .Build();
        var expander = (IMultiValuesWithMainValueSettingsEntry<ISingleValueSettingsEntry<object>>) entries[0];
        expander.MainValue.Value = ProxyType.Custom;
        ((ISingleValueSettingsEntry<string>) expander.Entries[0]).Value = "http://localhost:1234";

        Assert.AreEqual("http://localhost:1234", proxy);
        Assert.AreEqual(ProxyType.Custom, settings.ProxySettings.ProxyType);
        entries[0].LocalValueReset(new AppSettings());
        Assert.IsNull(proxy);
        Assert.AreEqual(new ProxySettings(), settings.ProxySettings);
    }

    [TestMethod]
    public void ExpanderShouldReadItsOwnMetadataAndResolveSubSettingsOnce()
    {
        var settings = new TestSettings();
        var entries = SettingsBuilder.CreateGroup(settings)
            .MultiValuesWithSwitch(t => t.Child, t => t.Enabled,
                children => children.String(t => t.Text))
            .Build();
        var expander = (IMultiValuesWithMainValueSettingsEntry<ISingleValueSettingsEntry<bool>>) entries[0];

        Assert.AreEqual(1, settings.ReadCount);
        Assert.AreEqual("Group header", entries[0].Header);
        Assert.AreEqual("Value header", expander.MainValue.Header);
        expander.MainValue.Value = true;
        ((ISingleValueSettingsEntry<string>) expander.Entries[0]).Value = "changed";
        Assert.IsTrue(settings.Child.Enabled);
        Assert.AreEqual("changed", settings.Child.Text);
    }

    [TestMethod]
    public void NullSubSettingsShouldBeRejected()
    {
        var settings = new ApplicationSettingsGroup { FileCache = null! };

        Assert.ThrowsExactly<InvalidOperationException>(() => SettingsBuilder.CreateGroup(settings)
            .MultiValues(t => t.FileCache, null));
        Assert.ThrowsExactly<InvalidOperationException>(() => SettingsBuilder.CreateGroup(settings)
            .MultiValuesWithSwitch(t => t.FileCache, t => t.LimitFileCacheSize, null));
    }

    [TestMethod]
    public void NestedSettingsShouldRoundTripThroughGeneratedYamlSerializer()
    {
        var settings = new AppSettings();
        settings.ApplicationSettings.HomePage.HomePageRows = 10;
        settings.BrowsingExperienceSettings.AutoPlay.IllustrationViewerAutoPlayInterval = 17;
        settings.ApplicationSettings.FileCache.LimitFileCacheSize = true;
        settings.ApplicationSettings.FileCache.FileCacheSizeLimitInMegabytes = 1234;
        settings.BrowsingExperienceSettings.ThumbnailLayout.IllustrationGridItemSize = 321;
        settings.SearchSettings.RankOptions.NovelRankOption = Mako.Global.Enum.RankOption.Week;
        settings.DownloadSettings.DownloadFormats.NovelDownloadFormat = "test-format";
        settings.NetworkSettings.PixivDomainFronting.EnablePixivDomainFronting = false;
        settings.NetworkSettings.GitHubDomainFronting.GitHubNameResolver = ["127.0.0.1"];
        settings.NetworkSettings.ProxySettings.Proxy = "http://localhost:1234";
        settings.NetworkSettings.ProxySettings.ProxyType = ProxyType.Custom;
        using var stream = new MemoryStream();

        YamlSerializer.Serialize(stream, settings, SettingsSerializerContext.Default.AppSettings);
        stream.Position = 0;
        var restored = YamlSerializer.Deserialize(stream, SettingsSerializerContext.Default.AppSettings)!;

        Assert.AreEqual(settings.ApplicationSettings.FileCache, restored.ApplicationSettings.FileCache);
        Assert.AreEqual(settings.ApplicationSettings.HomePage, restored.ApplicationSettings.HomePage);
        Assert.AreEqual(settings.BrowsingExperienceSettings.AutoPlay, restored.BrowsingExperienceSettings.AutoPlay);
        Assert.AreEqual(settings.BrowsingExperienceSettings.ThumbnailLayout, restored.BrowsingExperienceSettings.ThumbnailLayout);
        Assert.AreEqual(settings.SearchSettings.RankOptions, restored.SearchSettings.RankOptions);
        Assert.AreEqual(settings.DownloadSettings.DownloadFormats, restored.DownloadSettings.DownloadFormats);
        Assert.AreEqual(settings.NetworkSettings.ProxySettings, restored.NetworkSettings.ProxySettings);
        Assert.IsFalse(restored.NetworkSettings.PixivDomainFronting.EnablePixivDomainFronting);
        CollectionAssert.AreEqual(new[] { "127.0.0.1" }, restored.NetworkSettings.GitHubDomainFronting.GitHubNameResolver);
    }

    [TestMethod]
    public void HomePageAndAutoPlayExpandersShouldImportAndResetTheirChildren()
    {
        var settings = new AppSettings();
        var home = SettingsBuilder.CreateGroup(settings.ApplicationSettings)
            .MultiValues(t => t.HomePage, entries => entries
                .Int(t => t.HomePageRows, 1, 12, 1)
                .Int(t => t.HomePageColumns, 1, 12, 1)
                .Bool(t => t.HideHomePageToolbar)
                .Bool(t => t.HideHomePageCardTitle))
            .Build()[0];
        var autoPlay = SettingsBuilder.CreateGroup(settings.BrowsingExperienceSettings)
            .MultiValues(t => t.AutoPlay, entries => entries
                .Int(t => t.IllustrationViewerAutoPlayInterval, 1, 60, 1)
                .Enum(t => t.IllustrationViewerAutoPlayMode)
                .Enum(t => t.IllustrationViewerAutoPlayScope))
            .Build()[0];
        var imported = new AppSettings();
        imported.ApplicationSettings.HomePage = new()
        {
            HomePageRows = 9,
            HomePageColumns = 3,
            HideHomePageToolbar = true,
            HideHomePageCardTitle = true
        };
        imported.BrowsingExperienceSettings.AutoPlay = new()
        {
            IllustrationViewerAutoPlayInterval = 13,
            IllustrationViewerAutoPlayMode = IllustrationViewerAutoPlayMode.Loop,
            IllustrationViewerAutoPlayScope = IllustrationViewerAutoPlayScope.AllWorks
        };

        home.LocalValueReset(imported);
        autoPlay.LocalValueReset(imported);
        Assert.AreEqual(imported.ApplicationSettings.HomePage, settings.ApplicationSettings.HomePage);
        Assert.AreEqual(imported.BrowsingExperienceSettings.AutoPlay, settings.BrowsingExperienceSettings.AutoPlay);

        home.LocalValueReset(new AppSettings());
        autoPlay.LocalValueReset(new AppSettings());
        Assert.AreEqual(new HomePageSettings(), settings.ApplicationSettings.HomePage);
        Assert.AreEqual(new AutoPlaySettings(), settings.BrowsingExperienceSettings.AutoPlay);
    }

    private sealed class TestSettings
    {
        private readonly TestSubSettings _child = new();

        public int ReadCount { get; private set; }

        [SettingsEntry(Header = "Group header")]
        public TestSubSettings Child
        {
            get
            {
                ++ReadCount;
                return _child;
            }
        }
    }

    private sealed class TestSubSettings
    {
        [SettingsEntry(Header = "Value header")]
        public bool Enabled { get; set; }

        public string Text { get; set; } = "";
    }
}
