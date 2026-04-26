using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoSettingsPage;
using AutoSettingsPage.Models;
using AutoSettingsPage.WinUI;
using Microsoft.Graphics.Canvas.Text;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Controls.Settings;
using Pixeval.Extensions;
using Pixeval.Extensions.Common.Settings;
using Pixeval.Options;
using Pixeval.Pages.Misc;
using Pixeval.Settings.Models;
using Pixeval.Utilities;
using Windows.Foundation.Collections;
using WinUI3Utilities;
using Symbol = FluentIcons.Common.Symbol;

namespace Pixeval.Settings;

public static class LocalSettingsEntryHelper
{
    public static readonly SettingsEntryAttribute AutoUpdate = new(Symbol.Communication,
        nameof(SettingsPageResources.DownloadUpdateAutomaticallyEntryHeader),
        nameof(SettingsPageResources.DownloadUpdateAutomaticallyEntryDescription));

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
        typeof(AppSettings).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .SelectNotNull(f => f.GetCustomAttribute<SettingsEntryAttribute>()).ToArray());

    public static ISettingsValueConverter Converter { get; } = new SettingsValueConverter();

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
        SettingsEntryHelper.AvailableFonts = CanvasTextFormat.GetSystemFontFamilies();
        SettingsEntryAttribute.SettingsResourceKeysProvider = new SettingsResourceKeysProviderImpl();

        SettingsEntryHelper.FactoryDictionary.AddPredefined<AppSettings>()
            .Add<DateWithSwitchSettingsEntry<AppSettings>, DateWithSwitchSettingsCard>()
            .Add<ProxyAppSettingsEntry, ProxySettingsExpander>()
            .Add<LanguageAppSettingsEntry, LanguageSettingsCard>()
            .Add<IPSetSettingsEntry<AppSettings>, IPListInput>()
            .Add<DomainFrontingSettingsEntry<AppSettings>, DomainFrontingSettingsExpander>()
            .Add<DownloadMacroAppSettingsEntry, DownloadMacroSettingsExpander>()
            .Add<MultiValuesEntry<AppSettings>, MultiValuesSettingsExpander>()

            .Add<ExtensionSettingsEntry<IStringSettingsExtension, string>, StringSettingsCard>()
            .Add<ExtensionDoubleSettingsEntry, DoubleSettingsCard>()
            .Add<ExtensionIntSettingsEntry, DoubleSettingsCard>()
            .Add<ExtensionSettingsEntry<IBoolSettingsExtension, bool>, BoolSettingsCard>()
            .Add<ExtensionEnumSettingsEntry, EnumSettingsCard>()
            .Add<ExtensionSettingsEntry<IDateTimeOffsetSettingsExtension, DateTimeOffset>, DateSettingsCard>()
            .Add<ExtensionSettingsEntry<IStringsArraySettingsExtension, ObservableCollection<string>>, TokenizingSettingsExpander>()
            .Add<ExtensionSettingsEntry<IColorSettingsExtension, uint>, ColorSettingsCard>();
    }

    extension(ISettingsEntry entry)
    {
        public void LocalValueSaving(IPropertySet values, AppSettings appSettings)
        {

            if (entry is IReadOnlySingleValueSettingsEntry i)
                if (Converter.TryConvert(i.Value, out var result))
                    values[i.Token] = result;

            if (entry is IMultiValuesSettingsEntry m)
            {
                // WinUI 项目不会嵌套 IMultiValuesSettingsEntry
                foreach (var e in m.Entries)
                    if (e is IReadOnlySingleValueSettingsEntry s)
                        if (Converter.TryConvert(s.Value, out var result))
                            values[s.Token] = result;

                if (entry is DomainFrontingSettingsEntry<AppSettings> { Entries: { Count: 6 } entries })
                    foreach (var settingsEntry in entries)
                    {
                        // 若名称解析器有变更，则更新全局名称解析器
                        if (settingsEntry is not IPSetSettingsEntry<AppSettings> e)
                            return;

                        var nameResolver = settingsEntry.Token switch
                        {
                            nameof(AppSettings.PixivAppApiNameResolver) => appSettings.PixivAppApiNameResolver,
                            nameof(AppSettings.PixivImageNameResolver) => appSettings.PixivImageNameResolver,
                            nameof(AppSettings.PixivImageNameResolver2) => appSettings.PixivImageNameResolver2,
                            nameof(AppSettings.PixivOAuthNameResolver) => appSettings.PixivOAuthNameResolver,
                            nameof(AppSettings.PixivAccountNameResolver) => appSettings.PixivAccountNameResolver,
                            nameof(AppSettings.PixivWebApiNameResolver) => appSettings.PixivWebApiNameResolver,
                            _ => throw new ArgumentOutOfRangeException(nameof(settingsEntry.Token))
                        };

                        if (!nameResolver.SequenceEqual(e.Value))
                        {
                            AppInfo.SetNameResolvers(appSettings);
                            break;
                        }
                    }
            }
        }

        public void LocalValueReset(AppSettings appSettings)
        {
            if (entry is ISettingsValueReset<AppSettings> i)
                i.ValueReset(appSettings);

            if (entry is IMultiValuesSettingsEntry m)
            {
                // WinUI 项目不会嵌套 IMultiValuesSettingsEntry
                foreach (var e in m.Entries)
                    if (e is ISettingsValueReset<AppSettings> s)
                        s.ValueReset(appSettings);
            }
        }
    }

    extension<TSettings>(ISettingsGroupListBuilder<TSettings> builder)
    {
        public ISettingsGroupListBuilder<TSettings> NewGroup(
            SettingsEntryCategory header,
            string description = "",
            Symbol icon = default,
            Uri? descriptionUri = null,
            string? token = null,
            Action<ISettingsGroup>? config = null) =>
            builder.NewGroup(SettingsPageResources.GetResource(header), description, icon, descriptionUri, token, config);
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
                entry.Header = SettingsPageResources.GetResource(workType) ;
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
            Action<LanguageAppSettingsEntry>? config = null) =>
            builder.Add(new(), config);

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
        public string this[string resourceKey] => SettingsPageResources.GetResource(resourceKey);
    }
}
