namespace Pixeval.Pages.Misc;

public record LanguageModel(string DisplayName, string Name)
{
    public override string ToString() => DisplayName;

    public static LanguageModel DefaultLanguage { get; } = new(SettingsPageResources.LanguageSystemDefault, "");
}
