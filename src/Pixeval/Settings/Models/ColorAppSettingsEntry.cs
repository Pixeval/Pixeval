// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Linq.Expressions;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public partial class ColorAppSettingsEntry(
    AppSettings settings,
    Expression<Func<AppSettings, uint>> property)
    : SingleValueSettingsEntry<AppSettings, uint>(settings, property)
{
    public override ColorSettingsCard Element => new() { Entry = this };
}
