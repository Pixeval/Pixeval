namespace Pixeval.Settings;

public abstract class SingleValueSettingsEntryBase<TSettings>(TSettings settings)
    : ObservableSettingsEntryBase("", "", default)
{
    public TSettings Settings { get; } = settings;
}
