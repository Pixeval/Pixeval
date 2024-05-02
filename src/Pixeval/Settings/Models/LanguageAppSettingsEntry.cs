using System.Collections.Generic;
using System.Linq;
using Windows.Globalization;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;
using Pixeval.Pages.Misc;

namespace Pixeval.Settings.Models;

public class LanguageAppSettingsEntry(
    AppSettings appSettings)
    : ObservableSettingsEntryBase<AppSettings>(appSettings, "", "", default)
{
    public override LanguageSettingsCard Element => new() { Entry = this };

    public static IEnumerable<LanguageModel> AvailableLanguages { get; } = [LanguageModel.DefaultLanguage, new("简体中文", "zh-Hans-CN"), new("Русский", "ru"), new("English (United States)", "en-US")];

    public LanguageModel AppLanguage
    {
        get => AvailableLanguages.FirstOrDefault(t => t.Name == ApplicationLanguages.PrimaryLanguageOverride) ?? LanguageModel.DefaultLanguage;
        set => ApplicationLanguages.PrimaryLanguageOverride = value.Name;
    }

    public override void ValueReset()
    {
        AppLanguage = LanguageModel.DefaultLanguage;
        OnPropertyChanged(nameof(AppLanguage));
    }
}
