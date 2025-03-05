// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;
using Pixeval.Pages.Misc;
using Windows.Foundation.Collections;
using Windows.Globalization;

namespace Pixeval.Settings.Models;

public partial class LanguageAppSettingsEntry()
    : ObservableSettingsEntryBase("", "", default), IAppSettingEntry<AppSettings>
{
    public override LanguageSettingsCard Element => new() { Entry = this };

    public static IEnumerable<LanguageModel> AvailableLanguages { get; } = [LanguageModel.DefaultLanguage, LanguageModel.FromBcl47("zh-Hans"), LanguageModel.FromBcl47("ru-ru"), LanguageModel.FromBcl47("fr-fr"), LanguageModel.FromBcl47("en-us")];

    public static LanguageModel AppLanguage
    {
        get => AvailableLanguages.FirstOrDefault(t => t.Name == ApplicationLanguages.PrimaryLanguageOverride) ?? LanguageModel.DefaultLanguage;
        set => ApplicationLanguages.PrimaryLanguageOverride = value.Name;
    }

    public void ValueReset(AppSettings defaultSetting)
    {
        AppLanguage = LanguageModel.DefaultLanguage;
        OnPropertyChanged(nameof(AppLanguage));
    }

    public override void ValueSaving(IPropertySet values)
    {
    }
}
