using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Linq.Expressions;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public class FontAppSettingsEntry(
    AppSettings appSettings,
    Expression<Func<AppSettings, string>> property)
    : SingleValueSettingsEntry<AppSettings, string>(appSettings, property)
{
    public static IEnumerable<string> AvailableFonts { get; }

    public Action<string>? ValueChanged { get; set; }

    static FontAppSettingsEntry()
    {
        using var collection = new InstalledFontCollection();
        AvailableFonts = collection.Families.Select(t => t.Name);
    }

    public override FontSettingsCard Element => new() { Entry = this };
}
