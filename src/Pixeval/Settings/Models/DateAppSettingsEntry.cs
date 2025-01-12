// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Linq.Expressions;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public partial class DateAppSettingsEntry(
    AppSettings settings,
    Expression<Func<AppSettings, DateTimeOffset>> property)
    : SingleValueSettingsEntry<AppSettings, DateTimeOffset>(settings, property)
{
    public override DateSettingsCard Element => new() { Entry = this };
}
