// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Linq.Expressions;
using Microsoft.Graphics.Canvas.Text;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public partial class FontAppSettingsEntry(
    AppSettings settings,
    Expression<Func<AppSettings, string>> property)
    : StringAppSettingsEntry(settings, property)
{
    public override FontSettingsCard Element => new() { Entry = this };

    public static string[] AvailableFonts { get; } = CanvasTextFormat.GetSystemFontFamilies();
}
