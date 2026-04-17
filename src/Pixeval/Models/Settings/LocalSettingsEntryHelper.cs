using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoSettingsPage;
using AutoSettingsPage.Avalonia;
using AutoSettingsPage.Models;
using FluentIcons.Common;
using Pixeval.AppManagement;
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

    public static readonly SettingsEntryAttribute SignOut = new(Symbol.SignOut,
        nameof(SettingsPageResources.SignOutEntryHeader),
        nameof(SettingsPageResources.SignOutEntryDescription));

    public static readonly SettingsEntryAttribute ResetSettings = new(Symbol.Apps,
        nameof(SettingsPageResources.ResetDefaultSettingsEntryHeader),
        nameof(SettingsPageResources.ResetDefaultSettingsEntryDescription));

    public static readonly SettingsEntryAttribute DeleteHistories = new(Symbol.Delete,
        nameof(SettingsPageResources.DeleteHistoriesEntryHeader),
        null);

    public static readonly Lazy<IReadOnlyList<SettingsEntryAttribute>> LazyValues = new(() =>
        [.. typeof(AppSettings).GetProperties(BindingFlags.Instance | BindingFlags.Public).SelectNotNull(f => f.GetCustomAttribute<SettingsEntryAttribute>())]);

    public static SettingsEntryAttribute GetFromPropertyName(string propertyName)
    {
        return GetFromPropertyName<AppSettings>(propertyName);
    }

    public static SettingsEntryAttribute GetFromPropertyName<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(string propertyName)
    {
        return typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)
                   ?.GetCustomAttribute<SettingsEntryAttribute>() ??
               throw new ArgumentException(propertyName);
    }

    static LocalSettingsEntryHelper()
    {
        // SettingsEntryHelper.AvailableFonts = CanvasTextFormat.GetSystemFontFamilies();
        SettingsEntryAttribute.SettingsResourceKeysProvider = new SettingsResourceKeysProviderImpl();

        _ = SettingsEntryHelper.FactoryDictionary.AddPredefined<AppSettings>()
            .Add<CollectionSettingsEntry<AppSettings, string>, StringCollectionSettingsExpander>()
            .Add<DateWithSwitchSettingsEntry<AppSettings>, DateWithSwitchSettingsCard>()
            .Add<ProxyAppSettingsEntry, ProxySettingsExpander>()
            .Add<LanguageSettingsEntry<AppSettings>, LanguageSettingsCard>()
            .Add<IPSetSettingsEntry<AppSettings>, IPListInput>()
            .Add<DomainFrontingSettingsEntry<AppSettings>, DomainFrontingSettingsExpander>()
            .Add<DownloadMacroAppSettingsEntry, DownloadMacroSettingsExpander>()
            .Add<MultiValuesEntry<AppSettings>, MultiValuesSettingsExpander>()

            .Add<ExtensionSettingsEntry<IStringSettingsExtension, string>, StringSettingsCard>()
            .Add<ExtensionDoubleSettingsEntry, DoubleSettingsCard>()
            .Add<ExtensionIntSettingsEntry, DoubleSettingsCard>()
            .Add<ExtensionSettingsEntry<IBoolSettingsExtension, bool>, BoolSettingsCard>()
            .Add<ExtensionEnumSettingsEntry, EnumSettingsCard>()
            .Add<ExtensionSettingsEntry<IDateTimeOffsetSettingsExtension, DateTime>, DateSettingsCard>()
            .Add<ExtensionSettingsEntry<IStringsArraySettingsExtension, ObservableCollection<string>>, StringCollectionSettingsExpander>()
            .Add<ExtensionSettingsEntry<IColorSettingsExtension, uint>, ColorSettingsCard>();
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
            var item = SettingsEntryCategoryExtension.Items.First(t => Equals(t.Value, header));
            return builder.NewGroup(item.Description, description, item.Symbol, descriptionUri, token, config);
        }
    }

    extension<TSettings>(ISettingsGroupBuilder<TSettings> builder)
    {
        public ISettingsGroupBuilder<TSettings> Enum<TEnum>(
            WorkTypeEnum workType,
            Expression<Func<TSettings, TEnum>> property,
            IReadOnlyList<IReadOnlyStringPair<TEnum>> enumItems,
            Action<EnumSettingsEntry<TSettings, TEnum>>? config = null)
        {
            return builder.Add(new EnumSettingsEntry<TSettings, TEnum>(builder.Settings, property, enumItems), entry =>
            {
                entry.Header = I18NManager.GetResource(workType) ;
                entry.Description = "";
                entry.Icon = workType switch
                {
                    WorkTypeEnum.Illustration => Symbol.Image,
                    WorkTypeEnum.Manga => Symbol.ImageMultiple,
                    WorkTypeEnum.Ugoira => Symbol.Gif,
                    WorkTypeEnum.Novel => Symbol.BookOpen,
                    _ => throw new ArgumentOutOfRangeException(nameof(workType))
                };
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
    }

    extension(ISettingsGroupBuilder<AppSettings> builder)
    {
        public ISettingsGroupBuilder<AppSettings> Proxy(
            Action<ProxyAppSettingsEntry>? config = null) =>
            builder.Add(new(builder.Settings), config);

        public ISettingsGroupBuilder<AppSettings> DownloadMacro(
            Action<DownloadMacroAppSettingsEntry>? config = null) =>
            builder.Add(new(builder.Settings), config);
    }

    private class SettingsResourceKeysProviderImpl : ISettingsResourceKeysProvider
    {
        /// <inheritdoc />
        public string this[string resourceKey] => I18NManager.GetResource(resourceKey);
    }
}
