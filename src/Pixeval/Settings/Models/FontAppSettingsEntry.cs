using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public class FontAppSettingsEntry(
    AppSettings appSettings)
    : ObservableSettingsEntryBase<AppSettings>(appSettings, "", "", default)
{
    public static IEnumerable<string> AvailableFonts { get; }

    static FontAppSettingsEntry()
    {
        using var collection = new InstalledFontCollection();
        AvailableFonts = collection.Families.Select(t => t.Name);
    }

    public override FontSettingsCard Element => new() { Entry = this };

    public string AppFontFamilyName
    {
        get => Settings.AppFontFamilyName;
        set => Settings.AppFontFamilyName = value;
    }

    public override void ValueReset() => OnPropertyChanged(nameof(AppFontFamilyName));
}