// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Linq.Expressions;
using Microsoft.UI.Xaml;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public partial class StringAppSettingsEntry(
    AppSettings settings,
    Expression<Func<AppSettings, string>> property)
    : SingleValueSettingsEntry<AppSettings, string>(settings, property), IStringSettingsEntry
{
    public override FrameworkElement Element => new StringSettingsCard { Entry = this };

    public string? Placeholder { get; set; }

    public StringAppSettingsEntry(
        AppSettings settings,
        WorkTypeEnum workType,
        Expression<Func<AppSettings, string>> property)
        : this(settings, property)
    {
        Header = SubHeader(workType);
        HeaderIcon = SubHeaderIcon(workType);
    }
}
