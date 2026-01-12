// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Linq;
using Pixeval.AppManagement;
using Pixeval.Pages.Misc;
using Windows.Globalization;
using AutoSettingsPage;
using AutoSettingsPage.Models;
using FluentIcons.Common;

namespace Pixeval.Settings.Models;

public partial class LanguageAppSettingsEntry()
    : ObservableSettingsEntry("Language", LanguageEntryAttribute), ISettingsValueReset<AppSettings>
{
    public static SettingsEntryAttribute LanguageEntryAttribute { get; } = new(Symbol.LocalLanguage, nameof(SettingsPageResources.AppLanguageEntryHeader), nameof(SettingsPageResources.OpenLanguageSettingsHyperlinkButtonContent));

    /// <inheritdoc />
    public override Uri? DescriptionUri
    {
        get => new Uri("ms-settings:regionlanguage");
        set => throw new NotSupportedException();
    }

    public static IEnumerable<LanguageModel> AvailableLanguages { get; } = [LanguageModel.DefaultLanguage, LanguageModel.FromBcp47("zh-Hans"), LanguageModel.FromBcp47("ru-ru"), LanguageModel.FromBcp47("fr-fr"), LanguageModel.FromBcp47("en-us")];

    public LanguageModel Value
    {
        get => AvailableLanguages.FirstOrDefault(t => t.Name == ApplicationLanguages.PrimaryLanguageOverride) ?? LanguageModel.DefaultLanguage;
        set => ApplicationLanguages.PrimaryLanguageOverride = value.Name;
    }

    public void ValueReset(AppSettings defaultSetting)
    {
        Value = LanguageModel.DefaultLanguage;
        OnPropertyChanged(nameof(Value));
    }
}
