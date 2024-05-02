using System.Collections.ObjectModel;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public class TokenizingAppSettingsEntry(
    AppSettings appSettings)
    : ObservableSettingsEntryBase<AppSettings>(appSettings, "", "", default)
{
    public override TokenizingSettingsExpander Element => new() { Entry = this };

    public override void ValueReset()
    {
        BlockedTags = [.. Settings.BlockedTags];
        OnPropertyChanged(nameof(BlockedTags));
    }

    public override void ValueSaving() => Settings.BlockedTags = [.. BlockedTags];

    public ObservableCollection<string> BlockedTags { get; set; } = [.. appSettings.BlockedTags];
}