// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using FluentIcons.Common;
using Pixeval.AppManagement;
using Pixeval.Attributes;
using Pixeval.Controls.Settings;
using Windows.Foundation.Collections;
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
        var member = property.Body switch
        {
            UnaryExpression { Operand: MemberExpression member1 } => member1,
            MemberExpression member2 => member2,
            _ => ThrowHelper.Argument<Expression<Func<AppSettings, object>>, MemberExpression>(property)
        };

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
