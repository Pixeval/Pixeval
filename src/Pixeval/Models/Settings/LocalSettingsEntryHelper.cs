// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using AutoSettingsPage;
using AutoSettingsPage.Avalonia;
using AutoSettingsPage.Models;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using FluentIcons.Common;
using Mako;
using Mako.Global.Enum;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Extensions.Common.Settings;
using Pixeval.I18N;
using Pixeval.Models.Extensions;
using Pixeval.Models.Options;
using Pixeval.Models.Settings.Entries;
using Pixeval.Views.Settings;

namespace Pixeval.Models.Settings;

public static class LocalSettingsEntryHelper
{
    private static IDataTemplate LanguageValueTemplate { get; } =
        new FuncDataTemplate<ISingleValueSettingsEntry<string>>(static (entry, _) => new LanguageSettingsValue { Entry = entry });

    private static IDataTemplate IPSetValueTemplate { get; } =
        new FuncDataTemplate<ISingleValueSettingsEntry<ObservableCollection<string>>>(static (entry, _) => new IPListInput { Entry = entry });

    private static IDataTemplate FontValueTemplate { get; } =
        new FuncDataTemplate<ISingleValueSettingsEntry<ObservableCollection<string>>>(static (entry, _) => new FontSettingsValue { Entry = entry });

    private static IDataTemplate StringCollectionValueTemplate { get; } =
        new FuncDataTemplate<ISingleValueSettingsEntry<ObservableCollection<string>>>(static (entry, _) => new TokenizingBox
        {
            AllowDuplicateTokens = false,
            PlaceholderText = entry.Placeholder,
            TokenSeparator = ",",
            [!TokenizingBox.ItemsSourceProperty] = CompiledBinding.Create<ISingleValueSettingsEntry<ObservableCollection<string>>, ObservableCollection<string>>(
                value => value.Value,
                entry)
        });

    public static void Initialize()
    {
    }

    static LocalSettingsEntryHelper()
    {
        SettingsEntryAttribute.SettingsResourceKeysProvider = new SettingsResourceKeysProviderImpl();

        _ = SettingsEntryHelper.FactoryDictionary
            .AddPredefined()
            .AddOpenGeneric<ISingleValueSettingsEntry<ObservableCollection<string>>, StringCollectionSettingsExpander>(typeof(CollectionSettingsEntry<,>))
            .AddOpenGeneric<ISingleValueSettingsEntry<ObservableCollection<string>>, StringCollectionSettingsExpander>(typeof(FontSettingsEntry<>))
            .AddOpenGeneric<IMultiValuesWithMainValueSettingsEntry<ISingleValueSettingsEntry<bool>>, DomainFrontingSettingsExpander>(typeof(DomainFrontingSettingsEntry<>))
            .Add<DownloadMacroSettingsEntry, DownloadMacroSettingsExpander>()
            .Add<WorkSubscriptionsSettingsEntry, WorkSubscriptionsSettingsExpander>()
            .Add<BlockedUsersSettingsEntry, BlockedUsersSettingsExpander>()
            .Add<ExtensionSettingsEntry<IStringsArraySettingsExtension, ObservableCollection<string>>, StringCollectionSettingsExpander>();

        _ = SettingsEntryHelper.ValueFactoryDictionary
            .AddOpenGenericValue<ISingleValueSettingsEntry<string>>(
                typeof(LanguageSettingsEntry<>),
                LanguageValueTemplate)
            .AddOpenGenericValue<ISingleValueSettingsEntry<ObservableCollection<string>>>(
                typeof(IPSetSettingsEntry<>),
                IPSetValueTemplate)
            .AddOpenGenericValue<ISingleValueSettingsEntry<ObservableCollection<string>>>(
                typeof(FontSettingsEntry<>),
                FontValueTemplate)
            .AddOpenGenericValue<ISingleValueSettingsEntry<ObservableCollection<string>>>(
                typeof(CollectionSettingsEntry<,>),
                StringCollectionValueTemplate)
            .AddValue<ExtensionSettingsEntry<IStringsArraySettingsExtension, ObservableCollection<string>>>(
                StringCollectionValueTemplate);

        RegisterAttach<TargetFilter>(t =>
        {
            t.RegisterDirect(TargetFilter.ForAndroid, "Android");
            t.RegisterDirect(TargetFilter.ForIos, "iOS");
        });
        RegisterAttach<DomainFrontingType>(t =>
        {
            t.Register(DomainFrontingType.Fragmentation, EnumResources.DomainFrontingType.Fragmentation);
            // t.RegisterDirect(DomainFrontingType.Ech, "ECH");
            // t.Register(DomainFrontingType.Desync, EnumResources.DomainFrontingType.Desync);
        });
        RegisterAttach<SearchIllustrationTagMatchOption>(t =>
        {
            t.Register(SearchIllustrationTagMatchOption.PartialMatchForTags, EnumResources.SearchIllustrationTagMatchOption.PartialMatchForTags);
            t.Register(SearchIllustrationTagMatchOption.ExactMatchForTags, EnumResources.SearchIllustrationTagMatchOption.ExactMatchForTags);
            t.Register(SearchIllustrationTagMatchOption.TitleAndCaption, EnumResources.SearchIllustrationTagMatchOption.TitleAndCaption);
        });
        RegisterAttach<SearchNovelTagMatchOption>(t =>
        {
            t.Register(SearchNovelTagMatchOption.PartialMatchForTags, EnumResources.SearchNovelTagMatchOption.PartialMatchForTags);
            t.Register(SearchNovelTagMatchOption.ExactMatchForTags, EnumResources.SearchNovelTagMatchOption.ExactMatchForTags);
            t.Register(SearchNovelTagMatchOption.Text, EnumResources.SearchNovelTagMatchOption.Text);
            t.Register(SearchNovelTagMatchOption.Keyword, EnumResources.SearchNovelTagMatchOption.Keyword);
        });
        RegisterAttach<SearchIllustrationContentType>(t =>
        {
            t.Register(SearchIllustrationContentType.IllustrationAndMangaAndUgoira, EnumResources.SearchIllustrationContentType.IllustrationAndMangaAndUgoira);
            t.Register(SearchIllustrationContentType.IllustrationAndUgoira, EnumResources.SearchIllustrationContentType.IllustrationAndUgoira);
            t.Register(SearchIllustrationContentType.Illustration, EnumResources.SearchIllustrationContentType.Illustration);
            t.Register(SearchIllustrationContentType.Manga, EnumResources.SearchIllustrationContentType.Manga);
            t.Register(SearchIllustrationContentType.Ugoira, EnumResources.SearchIllustrationContentType.Ugoira);
        });
        RegisterAttach<SearchIllustrationRatioPattern>(t =>
        {
            t.Register(SearchIllustrationRatioPattern.All, EnumResources.SearchIllustrationRatioPattern.All);
            t.Register(SearchIllustrationRatioPattern.Landscape, EnumResources.SearchIllustrationRatioPattern.Landscape);
            t.Register(SearchIllustrationRatioPattern.Portrait, EnumResources.SearchIllustrationRatioPattern.Portrait);
            t.Register(SearchIllustrationRatioPattern.Square, EnumResources.SearchIllustrationRatioPattern.Square);
        });
        RegisterAttach<SearchNovelContentLengthOption>(t =>
        {
            t.Register(SearchNovelContentLengthOption.None, EnumResources.SearchNovelContentLengthOption.None);
            t.Register(SearchNovelContentLengthOption.TextLength, EnumResources.SearchNovelContentLengthOption.TextLength);
            t.Register(SearchNovelContentLengthOption.WordCount, EnumResources.SearchNovelContentLengthOption.WordCount);
            t.Register(SearchNovelContentLengthOption.ReadingTime, EnumResources.SearchNovelContentLengthOption.ReadingTime);
        });
        RegisterAttach<WorkSortOption>(t =>
        {
            t.Register(WorkSortOption.PublishDateDescending, Symbol.ArrowSortDownLines, EnumResources.WorkSortOption.PublishDateDescending);
            t.Register(WorkSortOption.PublishDateAscending, Symbol.ArrowSortUpLines, EnumResources.WorkSortOption.PublishDateAscending);
            t.Register(WorkSortOption.PopularityDescending, Symbol.ArrowTrendingSparkle, EnumResources.WorkSortOption.PopularityDescending);
        });
        RegisterAttach<PrivacyPolicy>(t =>
        {
            t.Register(PrivacyPolicy.Public, Symbol.Person, EnumResources.PrivacyPolicy.Public);
            t.Register(PrivacyPolicy.Private, Symbol.InPrivateAccount, EnumResources.PrivacyPolicy.Private);
        });
        RegisterAttach<FontWeight>(t =>
        {
            t.Register(FontWeight.Thin, Symbol.TextFont, EnumResources.FontWeight.Thin);
            t.Register(FontWeight.ExtraLight, Symbol.TextFont, EnumResources.FontWeight.ExtraLight);
            t.Register(FontWeight.Light, Symbol.TextFont, EnumResources.FontWeight.Light);
            t.Register(FontWeight.SemiLight, Symbol.TextFont, EnumResources.FontWeight.SemiLight);
            t.Register(FontWeight.Normal, Symbol.TextFont, EnumResources.FontWeight.Normal);
            t.Register(FontWeight.Medium, Symbol.TextFont, EnumResources.FontWeight.Medium);
            t.Register(FontWeight.SemiBold, Symbol.TextFont, EnumResources.FontWeight.SemiBold);
            t.Register(FontWeight.Bold, Symbol.TextFont, EnumResources.FontWeight.Bold);
            t.Register(FontWeight.ExtraBold, Symbol.TextFont, EnumResources.FontWeight.ExtraBold);
            t.Register(FontWeight.Black, Symbol.TextFont, EnumResources.FontWeight.Black);
            t.Register(FontWeight.ExtraBlack, Symbol.TextFont, EnumResources.FontWeight.ExtraBlack);
        });
        RegisterAttach<WorkType>(t =>
        {
            t.Register(WorkType.Illustration, Symbol.Image, EnumResources.WorkType.Illustration);
            t.Register(WorkType.Manga, Symbol.ImageStack, EnumResources.WorkType.Manga);
            t.Register(WorkType.Novel, Symbol.Book, EnumResources.WorkType.Novel);
        });
        RegisterAttach<SimpleWorkType>(t =>
        {
            t.Register(SimpleWorkType.Illustration, Symbol.Image, EnumResources.WorkType.Illustration);
            t.Register(SimpleWorkType.Novel, Symbol.Book, EnumResources.WorkType.Novel);
        });
        RegisterAttach<Orientation>(t =>
        {
            t.Register(Orientation.Horizontal, Symbol.ArrowBidirectionalLeftRight, EnumResources.Orientation.Horizontal);
            t.Register(Orientation.Vertical, Symbol.ArrowBidirectionalUpDown, EnumResources.Orientation.Vertical);
        });

        RegisterAttach<RankOption>(SimpleWorkType.Illustration, t =>
        {
            t.Register(RankOption.Day, EnumResources.RankOption.Day);
            t.Register(RankOption.Week, EnumResources.RankOption.Week);
            t.Register(RankOption.Month, EnumResources.RankOption.Month);
            t.Register(RankOption.DayMale, EnumResources.RankOption.DayMale);
            t.Register(RankOption.DayFemale, EnumResources.RankOption.DayFemale);
            t.Register(RankOption.DayManga, EnumResources.RankOption.DayManga);
            t.Register(RankOption.WeekManga, EnumResources.RankOption.WeekManga);
            t.Register(RankOption.MonthManga, EnumResources.RankOption.MonthManga);
            t.Register(RankOption.WeekOriginal, EnumResources.RankOption.WeekOriginal);
            t.Register(RankOption.WeekRookie, EnumResources.RankOption.WeekRookie);
            t.Register(RankOption.DayR18, EnumResources.RankOption.DayR18);
            t.Register(RankOption.DayMaleR18, EnumResources.RankOption.DayMaleR18);
            t.Register(RankOption.DayFemaleR18, EnumResources.RankOption.DayFemaleR18);
            t.Register(RankOption.WeekR18, EnumResources.RankOption.WeekR18);
            t.Register(RankOption.WeekR18G, EnumResources.RankOption.WeekR18G);
            t.Register(RankOption.DayAi, EnumResources.RankOption.DayAi);
            t.Register(RankOption.DayR18Ai, EnumResources.RankOption.DayR18Ai);
        });
        RegisterAttach<RankOption>(SimpleWorkType.Novel, t =>
        {
            t.Register(RankOption.Day, EnumResources.RankOption.Day);
            t.Register(RankOption.Week, EnumResources.RankOption.Week);
            t.Register(RankOption.DayMale, EnumResources.RankOption.DayMale);
            t.Register(RankOption.DayFemale, EnumResources.RankOption.DayFemale);
            t.Register(RankOption.WeekRookie, EnumResources.RankOption.WeekRookie);
            t.Register(RankOption.DayR18, EnumResources.RankOption.DayR18);
            t.Register(RankOption.DayMaleR18, EnumResources.RankOption.DayMaleR18);
            t.Register(RankOption.DayFemaleR18, EnumResources.RankOption.DayFemaleR18);
            t.Register(RankOption.WeekR18, EnumResources.RankOption.WeekR18);
            t.Register(RankOption.WeekR18G, EnumResources.RankOption.WeekR18G);
            t.Register(RankOption.WeekAi, EnumResources.RankOption.WeekAi);
            t.Register(RankOption.WeekAiR18, EnumResources.RankOption.WeekAiR18);
        });
    }

    public static Dictionary<object, IReadOnlyList<SymbolComboBoxItem>> RegisteredAttach { get; } = [];

    public static void RegisterAttach<TEnum>(Action<RegisterAttachHelper<TEnum>> config)
        where TEnum : struct, Enum
    {
        var list = new List<SymbolComboBoxItem>();
        var helper = new RegisterAttachHelper<TEnum>(list);
        config(helper);
        RegisteredAttach[typeof(TEnum)] = list;
    }

    public static void RegisterAttach<TEnum>(object key, Action<RegisterAttachHelper<TEnum>> config)
        where TEnum : struct, Enum
    {
        var list = new List<SymbolComboBoxItem>();
        var helper = new RegisterAttachHelper<TEnum>(list);
        config(helper);
        RegisteredAttach[key] = list;
    }

    extension(ISettingsEntry entry)
    {
        public void LocalValueReset(AppSettings resetAppSettings)
        {
            if (entry is ISettingsValueReset<AppSettings> i)
                i.ValueReset(resetAppSettings);
            if (entry is ISettingsValueReset<ApplicationSettingsGroup> application)
                application.ValueReset(resetAppSettings.ApplicationSettings);
            if (entry is ISettingsValueReset<NetworkSettingsGroup> network)
                network.ValueReset(resetAppSettings.NetworkSettings);
            if (entry is ISettingsValueReset<BrowsingExperienceSettingsGroup> browsingExperience)
                browsingExperience.ValueReset(resetAppSettings.BrowsingExperienceSettings);
            if (entry is ISettingsValueReset<SearchSettingsGroup> search)
                search.ValueReset(resetAppSettings.SearchSettings);
            if (entry is ISettingsValueReset<DownloadSettingsGroup> download)
                download.ValueReset(resetAppSettings.DownloadSettings);
#if PIXEVAL_MCP
            if (entry is ISettingsValueReset<McpSettingsGroup> mcp)
                mcp.ValueReset(resetAppSettings.McpSettings);
#endif
            if (entry is ISettingsValueReset<NovelSettingsGroup> novel)
                novel.ValueReset(resetAppSettings.NovelSettings);

            if (entry is IMultiValuesWithMainValueSettingsEntry multiValuesWithMainValue)
                multiValuesWithMainValue.MainValue.LocalValueReset(resetAppSettings);

            if (entry is IMultiValuesSettingsEntry m)
            {
                foreach (var e in m.Entries)
                    e.LocalValueReset(resetAppSettings);
            }
        }
    }

    private static Expression<Func<TSettings, object>> Transform<TSettings, TEnum>(Expression<Func<TSettings, TEnum>> property)
        where TEnum : struct, Enum =>
        Expression.Lambda<Func<TSettings, object>>(
            Expression.Convert(property.Body, typeof(object)),
            property.Parameters);

    extension<TSettings>(ISettingsGroupBuilder<TSettings> builder)
    {
        public ISettingsGroupBuilder<TSettings> Enum<TEnum>(
            Expression<Func<TSettings, TEnum>> property,
            Action<EnumSettingsEntry<TSettings, object>>? config = null)
            where TEnum : struct, Enum =>
            builder.Enum(Transform(property), SymbolComboBoxItem.GetValues<TEnum>(), config);

        public ISettingsGroupBuilder<TSettings> Enum<TEnum>(
            Expression<Func<TSettings, TEnum>> property,
            object key,
            Action<EnumSettingsEntry<TSettings, object>>? config = null)
            where TEnum : struct, Enum =>
            builder.Enum(Transform(property), SymbolComboBoxItem.GetValues<TEnum>(key), config);

        public ISettingsGroupBuilder<TSettings> Enum<TEnum>(
            WorkTypeEnum workType,
            Expression<Func<TSettings, TEnum>> property,
            Action<EnumSettingsEntry<TSettings, object>>? config = null)
            where TEnum : struct, Enum =>
            builder.Enum(workType, Transform(property), SymbolComboBoxItem.GetValues<TEnum>(), config);

        public ISettingsGroupBuilder<TSettings> Enum<TEnum>(
            WorkTypeEnum workType,
            Expression<Func<TSettings, TEnum>> property,
            object key,
            Action<EnumSettingsEntry<TSettings, object>>? config = null)
            where TEnum : struct, Enum =>
            builder.Enum(workType, Transform(property), SymbolComboBoxItem.GetValues<TEnum>(key), config);

        public ISettingsGroupBuilder<TSettings> Enum<TEnum>(
            WorkTypeEnum workType,
            Expression<Func<TSettings, TEnum>> property,
            IReadOnlyList<IReadOnlyStringPair<TEnum>> enumItems,
            Action<EnumSettingsEntry<TSettings, TEnum>>? config = null)
        {
            return builder.Add(new EnumSettingsEntry<TSettings, TEnum>(builder.Settings, property, enumItems), entry =>
            {
                entry.Description = "";
                (entry.Icon, var header) = workType switch
                {
                    WorkTypeEnum.Illustration => (Symbol.Image, EnumResources.WorkTypeEnum.Illustration),
                    WorkTypeEnum.Manga => (Symbol.ImageMultiple, EnumResources.WorkTypeEnum.Manga),
                    WorkTypeEnum.Ugoira => (Symbol.Gif, EnumResources.WorkTypeEnum.Ugoira),
                    WorkTypeEnum.Novel => (Symbol.BookOpen, EnumResources.WorkTypeEnum.Novel),
                    _ => throw new ArgumentOutOfRangeException(nameof(workType))
                };
                entry.Header = I18NManager.GetResource(header);
                config?.Invoke(entry);
            });
        }

        public ISettingsGroupBuilder<TSettings> MultiValuesWithMainValue<TEnum>(
            Expression<Func<TSettings, TEnum>> property,
            Action<ISettingsGroupBuilder<TSettings>>? configValues = null,
            Action<MultiValuesWithMainValueEntry<TSettings, EnumSettingsEntry<TSettings, object>>>? config = null)
            where TEnum : struct, Enum
        {
            var mainValue = new EnumSettingsEntry<TSettings, object>(
                builder.Settings,
                Transform(property),
                SymbolComboBoxItem.GetValues<TEnum>());
            return builder.MultiValuesWithMainValue(mainValue, configValues, config);
        }

        public ISettingsGroupBuilder<TSettings> Language(
            Expression<Func<TSettings, string>> property,
            Action<LanguageSettingsEntry<TSettings>>? config = null) =>
            builder.Add(new(builder.Settings, property), config);

        public ISettingsGroupBuilder<TSettings> IPSet(
            Expression<Func<TSettings, ObservableCollection<string>>> property,
            Action<IPSetSettingsEntry<TSettings>>? config = null) =>
            builder.Add(new(builder.Settings, property), config);

        public ISettingsGroupBuilder<TSettings> DomainFronting(
            Expression<Func<TSettings, bool>> property,
            Action<ISettingsGroupBuilder<TSettings>>? configValues,
            Action<DomainFrontingSettingsEntry<TSettings>>? config = null)
        {
            var simpleAddSettingsEntry = SettingsBuilder.CreateGroup(builder.Settings);
            configValues?.Invoke(simpleAddSettingsEntry);
            return builder.Add(new(builder.Settings, property, simpleAddSettingsEntry.Build()), config);
        }

        public ISettingsGroupBuilder<TSettings> Font(
            Expression<Func<TSettings, ObservableCollection<string>>> property,
            Action<FontSettingsEntry<TSettings>>? config = null) =>
            builder.Add(new(builder.Settings, property), config);
    }

    extension(ISettingsGroupBuilder<NetworkSettingsGroup> builder)
    {
        public ISettingsGroupBuilder<NetworkSettingsGroup> Proxy(
            Action<ProxySettingsEntry>? config = null) =>
            builder.Add(new(builder.Settings), config);
    }

    extension(ISettingsGroupBuilder<DownloadSettingsGroup> builder)
    {
        public ISettingsGroupBuilder<DownloadSettingsGroup> DownloadMacro(
            Expression<Func<DownloadSettingsGroup, string>> expression,
            Action<DownloadMacroSettingsEntry>? config = null) =>
            builder.Add(new(builder.Settings, expression), config);

        public ISettingsGroupBuilder<DownloadSettingsGroup> IllustrationDownloadFormat(
            Action<IllustrationDownloadFormatSettingsEntry>? config = null) =>
            builder.Add(new IllustrationDownloadFormatSettingsEntry(builder.Settings), config);

        public ISettingsGroupBuilder<DownloadSettingsGroup> UgoiraDownloadFormat(
            Action<UgoiraDownloadFormatSettingsEntry>? config = null) =>
            builder.Add(new UgoiraDownloadFormatSettingsEntry(builder.Settings), config);

        public ISettingsGroupBuilder<DownloadSettingsGroup> NovelDownloadFormat(
            Action<NovelDownloadFormatSettingsEntry>? config = null) =>
            builder.Add(new NovelDownloadFormatSettingsEntry(builder.Settings), config);

        public ISettingsGroupBuilder<DownloadSettingsGroup> WorkSubscriptions(
            Expression<Func<DownloadSettingsGroup, byte>> expression,
            Action<WorkSubscriptionsSettingsEntry>? config = null) =>
            builder.Add(new(expression), config);
    }

    extension(ISettingsGroupBuilder<BrowsingExperienceSettingsGroup> builder)
    {
        public ISettingsGroupBuilder<BrowsingExperienceSettingsGroup> BlockedUsers(
            Expression<Func<BrowsingExperienceSettingsGroup, byte>> expression,
            Action<BlockedUsersSettingsEntry>? config = null) =>
            builder.Add(new(expression), config);
    }

    private class SettingsResourceKeysProviderImpl : ISettingsResourceKeysProvider
    {
        /// <inheritdoc />
        public string this[string resourceKey] => I18NManager.GetResource(resourceKey);
    }
}

public class RegisterAttachHelper<TEnum>(IList<SymbolComboBoxItem> list)
    where TEnum : struct, Enum
{
    public void RegisterDirect(TEnum value, Symbol symbol, string resource) => list.Add(new SymbolComboBoxItem(value, resource, symbol));

    public void RegisterDirect(TEnum value, string resource) => RegisterDirect(value, default, resource);

    public void Register(TEnum value, Symbol symbol, string key) => RegisterDirect(value, symbol, I18NManager.GetResource(key));

    public void Register(TEnum value, string key) => Register(value, default, key);
}
