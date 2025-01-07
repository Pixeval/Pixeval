// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.UI.Xaml;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public partial class EnumAppSettingsEntry(
    AppSettings settings,
    Expression<Func<AppSettings, object>> property,
    IReadOnlyList<StringRepresentableItem> array)
    : SingleValueSettingsEntry<AppSettings, object>(settings, property), IEnumSettingsEntry
{
    public override FrameworkElement Element => new EnumSettingsCard { Entry = this };

    public IReadOnlyList<StringRepresentableItem> EnumItems { get; set; } = array;

    public EnumAppSettingsEntry(
        AppSettings settings,
        WorkTypeEnum workType,
        Expression<Func<AppSettings, object>> property,
        IReadOnlyList<StringRepresentableItem> array)
        : this(settings, property, array)
    {
        Header = SubHeader(workType);
        HeaderIcon = SubHeaderIcon(workType);
    }
}
