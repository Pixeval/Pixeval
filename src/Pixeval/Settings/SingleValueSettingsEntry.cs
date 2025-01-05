#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/SingleValueSettingsEntry.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Pixeval.Attributes;
using WinUI3Utilities;

namespace Pixeval.Settings;

public abstract class SingleValueSettingsEntry<TSettings, TValue> 
    : SingleValueSettingsEntryBase<TValue>, ISingleValueSettingsEntry<TValue>, IAppSettingEntry<TSettings>
{
    protected SingleValueSettingsEntry(TSettings settings,
        Expression<Func<TSettings, object>> property,
        Func<TSettings, TValue> getter,
        Action<TSettings, TValue> setter) : base("", "", default)
    {
        Settings = settings;
        _getter = getter;
        _setter = setter;
        // t => (T)t.A
        if (property.Body is not MemberExpression member)
        {
            ThrowHelper.Argument(property);
            return;
        }
        Token = member.Member.Name;
        Attribute = member.Member.GetCustomAttribute<SettingsEntryAttribute>();

        if (Attribute is { } attribute)
        {
            Header = attribute.LocalizedResourceHeader;
            Description = attribute.LocalizedResourceDescription;
            HeaderIcon = attribute.Symbol;
        }
    }

    protected SingleValueSettingsEntry(TSettings settings,
        Expression<Func<TSettings, TValue>> property) : base("", "", default)
    {
        Settings = settings;
        (_getter, _setter, var member) = GetSettingsEntryInfo(property);
        Token = member.Member.Name;
        Attribute = member.Member.GetCustomAttribute<SettingsEntryAttribute>();

        if (Attribute is { } attribute)
        {
            Header = attribute.LocalizedResourceHeader;
            Description = attribute.LocalizedResourceDescription;
            HeaderIcon = attribute.Symbol;
        }
    }

    public override string Token { get; }

    public SettingsEntryAttribute? Attribute { get; }

    public Action<TValue>? ValueChanged { get; set; }

    public TSettings Settings { get; }

    public override TValue Value
    {
        get => _getter(Settings);
        set
        {
            if (EqualityComparer<TValue>.Default.Equals(Value, value))
                return;
            _setter(Settings, value);
            OnPropertyChanged();
            ValueChanged?.Invoke(Value);
        }
    }

    public virtual void ValueReset(TSettings defaultSettings) => Value = _getter(defaultSettings);

    private readonly Func<TSettings, TValue> _getter;

    private readonly Action<TSettings, TValue> _setter;

    protected SettingsEntryInfo<TSettings1, TValue1> GetSettingsEntryInfo<TSettings1, TValue1>(
        Expression<Func<TSettings1, TValue1>> property)
    {
        if (property is not { Parameters: [{ } parameter] })
        {
            return ThrowHelper
                .Argument<Expression<Func<TSettings1, TValue1>>, SettingsEntryInfo<TSettings1, TValue1>>(property);
        }

        var propertyValue = Expression.Parameter(typeof(TValue1));
        BinaryExpression setPropertyValue;
        Expression getPropertyValue;
        MemberExpression member;
        switch (property.Body)
        {
            // t => (T)t.A
            case UnaryExpression
            {
                Operand: MemberExpression member1
            } body:
            {
                // t => (T)t.A
                getPropertyValue = body;
                // (t, v) => t.A = (T)v
                setPropertyValue = Expression.Assign(member1, Expression.Convert(propertyValue, member1.Type));
                // t => t.A
                member = member1;
                break;
            }
            // t => t.A
            case MemberExpression member2:
            {
                // t => t.A
                getPropertyValue = member = member2;
                // (t, v) => t.A = v
                setPropertyValue = Expression.Assign(member2, propertyValue);
                break;
            }
            default:
                return ThrowHelper
                    .Argument<Expression<Func<TSettings1, TValue1>>, SettingsEntryInfo<TSettings1, TValue1>>(property);
        }

        var getter = Expression.Lambda<Func<TSettings1, TValue1>>(getPropertyValue, parameter).Compile();
        var setter = Expression.Lambda<Action<TSettings1, TValue1>>(setPropertyValue, parameter, propertyValue).Compile();
        return new(getter, setter, member);
    }

    public record SettingsEntryInfo<TSettings1, TValue1>(
        Func<TSettings1, TValue1> Getter,
        Action<TSettings1, TValue1> Setter,
        MemberExpression Member);
}
