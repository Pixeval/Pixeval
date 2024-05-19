using System;
using System.Drawing.Text;
using System.Linq;
using System.Linq.Expressions;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;
using Pixeval.Utilities;

namespace Pixeval.Settings.Models;

public partial class FontAppSettingsEntry(
    AppSettings appSettings,
    Expression<Func<AppSettings, string>> property)
    : StringAppSettingsEntry(appSettings, property)
{
    public override FontSettingsCard Element => new() { Entry = this };

    public static string[] AvailableFonts { get; } = new InstalledFontCollection().Using(t => t.Families.Select(t => t.Name).ToArray());
}
