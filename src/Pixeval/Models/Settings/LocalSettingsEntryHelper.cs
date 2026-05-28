// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoSettingsPage;
using AutoSettingsPage.Avalonia;
using AutoSettingsPage.Models;
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
using Pixeval.Utilities;
using Pixeval.Views.Settings;

namespace Pixeval.Models.Settings;

public static class LocalSettingsEntryHelper
{
    public static void Initialize()
    {
    }

    public static readonly Lazy<IReadOnlyList<SettingsEntryAttribute>> LazyValues = new(() =>
        [.. typeof(AppSettings).GetProperties(BindingFlags.Instance | BindingFlags.Public).SelectNotNull(f => f.GetCustomAttribute<SettingsEntryAttribute>())]);

    static LocalSettingsEntryHelper()
    {
        SettingsEntryAttribute.SettingsResourceKeysProvider = new SettingsResourceKeysProviderImpl();

        _ = SettingsEntryHelper.FactoryDictionary.AddPredefined<AppSettings>()
            .Add<CollectionSettingsEntry<AppSettings, string>, StringCollectionSettingsExpander>()
            .Add<DateWithSwitchSettingsEntry<AppSettings>, DateWithSwitchSettingsCard>()
            .Add<ProxyAppSettingsEntry, ProxySettingsExpander>()
            .Add<LanguageSettingsEntry<AppSettings>, LanguageSettingsCard>()
            .Add<IPSetSettingsEntry<AppSettings>, IPListInput>()
            .Add<DomainFrontingSettingsEntry<AppSettings>, DomainFrontingSettingsExpander>()
            .Add<DownloadMacroAppSettingsEntry, DownloadMacroSettingsExpander>()
            .Add<IllustrationDownloadFormatSettingsEntry, EnumSettingsCard>()
            .Add<NovelDownloadFormatSettingsEntry, EnumSettingsCard>()
            .Add<UgoiraDownloadFormatSettingsEntry, EnumSettingsCard>()
            .Add<WorkSubscriptionsSettingsEntry, WorkSubscriptionsSettingsExpander>()
            .Add<MultiValuesEntry<AppSettings>, MultiValuesSettingsExpander>()
            .Add<FontSettingsEntry<AppSettings>, FontSettingsExpander>()

            .Add<ExtensionSettingsEntry<IStringSettingsExtension, string>, StringSettingsCard>()
            .Add<ExtensionDoubleSettingsEntry, DoubleSettingsCard>()
            .Add<ExtensionIntSettingsEntry, DoubleSettingsCard>()
            .Add<ExtensionSettingsEntry<IBoolSettingsExtension, bool>, BoolSettingsCard>()
            .Add<ExtensionEnumSettingsEntry, EnumSettingsCard>()
            .Add<ExtensionSettingsEntry<IDateTimeOffsetSettingsExtension, DateTime>, DateSettingsCard>()
            .Add<ExtensionSettingsEntry<IStringsArraySettingsExtension, ObservableCollection<string>>, StringCollectionSettingsExpander>()
            .Add<ExtensionSettingsEntry<IColorSettingsExtension, uint>, ColorSettingsCard>();

        RegisterAttach<TargetFilter>(t =>
        {
            t.RegisterDirect(TargetFilter.ForAndroid, "Android");
            t.RegisterDirect(TargetFilter.ForIos, "iOS");
        });
        RegisterAttach<DomainFrontingType>(t =>
        {
            t.Register(DomainFrontingType.Fragmentation, EnumResources.DomainFrontingTypeFragmentation);
            t.RegisterDirect(DomainFrontingType.Ech, "ECH");
            t.Register(DomainFrontingType.Desync, EnumResources.DomainFrontingTypeDesync);
        });
        RegisterAttach<SearchIllustrationTagMatchOption>(t =>
        {
            t.Register(SearchIllustrationTagMatchOption.PartialMatchForTags, EnumResources.SearchIllustrationTagMatchOptionPartialMatchForTags);
            t.Register(SearchIllustrationTagMatchOption.ExactMatchForTags, EnumResources.SearchIllustrationTagMatchOptionExactMatchForTags);
            t.Register(SearchIllustrationTagMatchOption.TitleAndCaption, EnumResources.SearchIllustrationTagMatchOptionTitleAndCaption);
        });
        RegisterAttach<SearchNovelTagMatchOption>(t =>
        {
            t.Register(SearchNovelTagMatchOption.PartialMatchForTags, EnumResources.SearchNovelTagMatchOptionPartialMatchForTags);
            t.Register(SearchNovelTagMatchOption.ExactMatchForTags, EnumResources.SearchNovelTagMatchOptionExactMatchForTags);
            t.Register(SearchNovelTagMatchOption.Text, EnumResources.SearchNovelTagMatchOptionText);
            t.Register(SearchNovelTagMatchOption.Keyword, EnumResources.SearchNovelTagMatchOptionKeyword);
        });
        RegisterAttach<SearchIllustrationContentType>(t =>
        {
            t.Register(SearchIllustrationContentType.IllustrationAndMangaAndUgoira, EnumResources.SearchIllustrationContentTypeIllustrationAndMangaAndUgoira);
            t.Register(SearchIllustrationContentType.IllustrationAndUgoira, EnumResources.SearchIllustrationContentTypeIllustrationAndUgoira);
            t.Register(SearchIllustrationContentType.Illustration, EnumResources.SearchIllustrationContentTypeIllustration);
            t.Register(SearchIllustrationContentType.Manga, EnumResources.SearchIllustrationContentTypeManga);
            t.Register(SearchIllustrationContentType.Ugoira, EnumResources.SearchIllustrationContentTypeUgoira);
        });
        RegisterAttach<SearchIllustrationRatioPattern>(t =>
        {
            t.Register(SearchIllustrationRatioPattern.All, EnumResources.SearchIllustrationRatioPatternAll);
            t.Register(SearchIllustrationRatioPattern.Landscape, EnumResources.SearchIllustrationRatioPatternLandscape);
            t.Register(SearchIllustrationRatioPattern.Portrait, EnumResources.SearchIllustrationRatioPatternPortrait);
            t.Register(SearchIllustrationRatioPattern.Square, EnumResources.SearchIllustrationRatioPatternSquare);
        });
        RegisterAttach<SearchNovelContentLengthOption>(t =>
        {
            t.Register(SearchNovelContentLengthOption.None, EnumResources.SearchNovelContentLengthOptionNone);
            t.Register(SearchNovelContentLengthOption.TextLength, EnumResources.SearchNovelContentLengthOptionTextLength);
            t.Register(SearchNovelContentLengthOption.WordCount, EnumResources.SearchNovelContentLengthOptionWordCount);
            t.Register(SearchNovelContentLengthOption.ReadingTime, EnumResources.SearchNovelContentLengthOptionReadingTime);
        });
        RegisterAttach<WorkSortOption>(t =>
        {
            t.Register(WorkSortOption.PublishDateDescending, Symbol.ArrowSortDownLines, EnumResources.WorkSortOptionPublishDateDescending);
            t.Register(WorkSortOption.PublishDateAscending, Symbol.ArrowSortUpLines, EnumResources.WorkSortOptionPublishDateAscending);
            t.Register(WorkSortOption.PopularityDescending, Symbol.ArrowTrendingSparkle, EnumResources.WorkSortOptionPopularityDescending);
        });
        RegisterAttach<PrivacyPolicy>(t =>
        {
            t.Register(PrivacyPolicy.Public, Symbol.Person, EnumResources.PrivacyPolicyPublic);
            t.Register(PrivacyPolicy.Private, Symbol.InPrivateAccount, EnumResources.PrivacyPolicyPrivate);
        });
        RegisterAttach<FontWeight>(t =>
        {
            t.Register(FontWeight.Thin, Symbol.TextFont, EnumResources.FontWeightThin);
            t.Register(FontWeight.ExtraLight, Symbol.TextFont, EnumResources.FontWeightExtraLight);
            t.Register(FontWeight.Light, Symbol.TextFont, EnumResources.FontWeightLight);
            t.Register(FontWeight.Normal, Symbol.TextFont, EnumResources.FontWeightNormal);
            t.Register(FontWeight.Medium, Symbol.TextFont, EnumResources.FontWeightMedium);
            t.Register(FontWeight.SemiBold, Symbol.TextFont, EnumResources.FontWeightSemiBold);
            t.Register(FontWeight.Bold, Symbol.TextFont, EnumResources.FontWeightBold);
            t.Register(FontWeight.ExtraBold, Symbol.TextFont, EnumResources.FontWeightExtraBold);
            t.Register(FontWeight.Black, Symbol.TextFont, EnumResources.FontWeightBlack);
        });
        RegisterAttach<WorkType>(t =>
        {
            t.Register(WorkType.Illustration, Symbol.Image, EnumResources.WorkTypeIllustration);
            t.Register(WorkType.Manga, Symbol.ImageStack, EnumResources.WorkTypeManga);
            t.Register(WorkType.Novel, Symbol.Book, EnumResources.WorkTypeNovel);
        });
        RegisterAttach<SimpleWorkType>(t =>
        {
            t.Register(SimpleWorkType.IllustrationAndManga, Symbol.Image, EnumResources.SimpleWorkTypeIllustrationAndManga);
            t.Register(SimpleWorkType.Novel, Symbol.Book, EnumResources.SimpleWorkTypeNovel);
        });

        RegisterAttach<RankOption>(SimpleWorkType.IllustrationAndManga, t =>
        {
            t.Register(RankOption.Day, EnumResources.RankOptionDay);
            t.Register(RankOption.Week, EnumResources.RankOptionWeek);
            t.Register(RankOption.Month, EnumResources.RankOptionMonth);
            t.Register(RankOption.DayMale, EnumResources.RankOptionDayMale);
            t.Register(RankOption.DayFemale, EnumResources.RankOptionDayFemale);
            t.Register(RankOption.DayManga, EnumResources.RankOptionDayManga);
            t.Register(RankOption.WeekManga, EnumResources.RankOptionWeekManga);
            t.Register(RankOption.MonthManga, EnumResources.RankOptionMonthManga);
            t.Register(RankOption.WeekOriginal, EnumResources.RankOptionWeekOriginal);
            t.Register(RankOption.WeekRookie, EnumResources.RankOptionWeekRookie);
            t.Register(RankOption.DayR18, EnumResources.RankOptionDayR18);
            t.Register(RankOption.DayMaleR18, EnumResources.RankOptionDayMaleR18);
            t.Register(RankOption.DayFemaleR18, EnumResources.RankOptionDayFemaleR18);
            t.Register(RankOption.WeekR18, EnumResources.RankOptionWeekR18);
            t.Register(RankOption.WeekR18G, EnumResources.RankOptionWeekR18G);
            t.Register(RankOption.DayAi, EnumResources.RankOptionDayAi);
            t.Register(RankOption.DayR18Ai, EnumResources.RankOptionDayR18Ai);
        });
        RegisterAttach<RankOption>(SimpleWorkType.Novel, t =>
        {
            t.Register(RankOption.Day, EnumResources.RankOptionDay);
            t.Register(RankOption.Week, EnumResources.RankOptionWeek);
            t.Register(RankOption.DayMale, EnumResources.RankOptionDayMale);
            t.Register(RankOption.DayFemale, EnumResources.RankOptionDayFemale);
            t.Register(RankOption.WeekRookie, EnumResources.RankOptionWeekRookie);
            t.Register(RankOption.DayR18, EnumResources.RankOptionDayR18);
            t.Register(RankOption.DayMaleR18, EnumResources.RankOptionDayMaleR18);
            t.Register(RankOption.DayFemaleR18, EnumResources.RankOptionDayFemaleR18);
            t.Register(RankOption.WeekR18, EnumResources.RankOptionWeekR18);
            t.Register(RankOption.WeekR18G, EnumResources.RankOptionWeekR18G);
            t.Register(RankOption.WeekAi, EnumResources.RankOptionWeekAi);
            t.Register(RankOption.WeekAiR18, EnumResources.RankOptionWeekAiR18);
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

            if (entry is IMultiValuesSettingsEntry m)
            {
                // 项目暂不会嵌套 IMultiValuesSettingsEntry
                foreach (var e in m.Entries)
                    if (e is ISettingsValueReset<AppSettings> s)
                        s.ValueReset(resetAppSettings);
            }
        }
    }

    extension<TSettings>(ISettingsGroupListBuilder<TSettings> builder)
    {
        public ISettingsGroupListBuilder<TSettings> NewGroup(
            SettingsEntryCategory header,
            string description = "",
            Uri? descriptionUri = null,
            string? token = null,
            Action<ISettingsGroup>? config = null)
        {
            var item = SymbolComboBoxItem.GetValues<SettingsEntryCategory>().First(t => Equals(t.Value, header));
            return builder.NewGroup(item.Description, description, item.Symbol, descriptionUri, token, config);
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
                    WorkTypeEnum.Illustration => (Symbol.Image, EnumResources.WorkTypeEnumIllustration),
                    WorkTypeEnum.Manga => (Symbol.ImageMultiple, EnumResources.WorkTypeEnumManga),
                    WorkTypeEnum.Ugoira => (Symbol.Gif, EnumResources.WorkTypeEnumUgoira),
                    WorkTypeEnum.Novel => (Symbol.BookOpen, EnumResources.WorkTypeEnumNovel),
                    _ => throw new ArgumentOutOfRangeException(nameof(workType))
                };
                entry.Header = I18NManager.GetResource(header);
                config?.Invoke(entry);
            });
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

        public ISettingsGroupBuilder<TSettings> DateWithSwitch(
            Expression<Func<TSettings, bool>> property,
            Action<ISettingsGroupBuilder<TSettings>>? configValues,
            Action<DateWithSwitchSettingsEntry<TSettings>>? config = null)
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

    extension(ISettingsGroupBuilder<AppSettings> builder)
    {
        public ISettingsGroupBuilder<AppSettings> Proxy(
            Action<ProxyAppSettingsEntry>? config = null) =>
            builder.Add(new(builder.Settings), config);

        public ISettingsGroupBuilder<AppSettings> DownloadMacro(
            Action<DownloadMacroAppSettingsEntry>? config = null) =>
            builder.Add(new(builder.Settings), config);

        public ISettingsGroupBuilder<AppSettings> IllustrationDownloadFormat(
            Action<IllustrationDownloadFormatSettingsEntry>? config = null) =>
            builder.Add(new IllustrationDownloadFormatSettingsEntry(builder.Settings), config);

        public ISettingsGroupBuilder<AppSettings> NovelDownloadFormat(
            Action<NovelDownloadFormatSettingsEntry>? config = null) =>
            builder.Add(new NovelDownloadFormatSettingsEntry(builder.Settings), config);

        public ISettingsGroupBuilder<AppSettings> UgoiraDownloadFormat(
            Action<UgoiraDownloadFormatSettingsEntry>? config = null) =>
            builder.Add(new UgoiraDownloadFormatSettingsEntry(builder.Settings), config);

        public ISettingsGroupBuilder<AppSettings> WorkSubscriptions(
            Action<WorkSubscriptionsSettingsEntry>? config = null) =>
            builder.Add(new(), config);
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
