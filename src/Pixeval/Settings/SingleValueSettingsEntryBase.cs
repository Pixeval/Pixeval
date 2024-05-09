namespace Pixeval.Settings;

public abstract class SingleValueSettingsEntryBase<TSettings>(TSettings settings)
    : ObservableSettingsEntryBase<TSettings>(settings, "", "", default);
