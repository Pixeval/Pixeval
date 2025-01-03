using System;
using System.Linq.Expressions;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public partial class DateAppSettingsEntry(
    AppSettings appSettings,
    Expression<Func<AppSettings, DateTimeOffset>> property)
    : SingleValueSettingsEntry<AppSettings, DateTimeOffset>(appSettings, property)
{
    public override DateSettingsCard Element => new() { Entry = this };
}
