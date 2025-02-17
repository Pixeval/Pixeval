// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Windows.Foundation.Collections;
using FluentIcons.Common;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;
using System.Linq.Expressions;
using System;
using System.Reflection;
using Pixeval.Attributes;
using WinUI3Utilities;

namespace Pixeval.Settings;

public class MultiValuesEntry(
    string header,
    string description,
    Symbol headerIcon,
    IReadOnlyList<IAppSettingEntry<AppSettings>> entries) 
    : SettingsEntryBase(header, description, headerIcon), IAppSettingEntry<AppSettings>
{
    /// <summary>
    /// 只从<paramref name="property"/>取出<see cref="SettingsEntryAttribute"/>元数据
    /// </summary>
    /// <param name="property"></param>
    /// <param name="entries"></param>
    public MultiValuesEntry(Expression<Func<AppSettings, object>> property, IReadOnlyList<IAppSettingEntry<AppSettings>> entries)
        : this("", "", default, entries)
    {
        // t => (T)t.A
        if (property.Body is not MemberExpression member)
        {
            ThrowHelper.Argument(property);
            return;
        }

        if (member.Member.GetCustomAttribute<SettingsEntryAttribute>() is { } attribute)
        {
            Header = attribute.LocalizedResourceHeader;
            Description = attribute.LocalizedResourceDescription;
            HeaderIcon = attribute.Symbol;
        }
    }

    public override MultiValuesAppSettingsExpander Element => new() { Entry = this };

    public IReadOnlyList<IAppSettingEntry<AppSettings>> Entries { get; } = entries;

    public void ValueReset(AppSettings defaultSettings)
    {
        foreach (var entry in Entries)
            entry.ValueReset(defaultSettings);
    }

    public override void ValueSaving(IPropertySet values)
    {
        foreach (var entry in Entries)
            entry.ValueSaving(values);
    }
}
