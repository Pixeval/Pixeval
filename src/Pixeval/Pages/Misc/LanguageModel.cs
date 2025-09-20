// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Windows.Globalization;

namespace Pixeval.Pages.Misc;

public record LanguageModel(string DisplayName, string Name)
{
    public override string ToString() => DisplayName;

    public static LanguageModel DefaultLanguage { get; } = new(SettingsPageResources.LanguageSystemDefault, "");

    public static LanguageModel FromBcp47(string bcp47)
    {
        var language = new Language(bcp47);
        return new LanguageModel(language.NativeName, language.LanguageTag);
    }
}
