// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Linq.Expressions;
using Microsoft.UI.Xaml;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public partial class BoolAppSettingsEntry(
    AppSettings settings,
    Expression<Func<AppSettings, bool>> property)
    : SingleValueSettingsEntry<AppSettings, bool>(settings, property)
{
    public override FrameworkElement Element => new BoolSettingsCard { Entry = this };

    public BoolAppSettingsEntry(
        AppSettings settings,
        WorkTypeEnum workType,
        Expression<Func<AppSettings, bool>> property)
        : this(settings, property)
    {
        Header = SubHeader(workType);
        HeaderIcon = SubHeaderIcon(workType);
    }
}
