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

public abstract class SingleValueSettingsEntry<TSettings, TValue> : SingleValueSettingsEntryBase<TSettings>
{
    protected SingleValueSettingsEntry(TSettings settings,
        Expression<Func<TSettings, TValue>> property) : base(settings)
    {
        if (property is not { Parameters: [{ } parameter] })
        {
            ThrowHelper.Argument(property);
            return;
        }

        var propertyValue = Expression.Parameter(typeof(TValue));
        BinaryExpression setPropertyValue;
        Expression getPropertyValue;
        MemberExpression member;
        switch (property.Body)
        {
            case UnaryExpression
            {
                Operand: MemberExpression member1
            } body:
            {
                getPropertyValue = body;
                setPropertyValue = Expression.Assign(member1, Expression.Convert(propertyValue, member1.Type));
                member = member1;
                break;
            }
            case MemberExpression member2:
            {
                getPropertyValue = member = member2;
                setPropertyValue = Expression.Assign(member2, propertyValue);
                break;
            }
            default:
                ThrowHelper.Argument(property);
                return;
        }

        _getter = Expression.Lambda<Func<TSettings, TValue>>(getPropertyValue, parameter).Compile();
        _setter = Expression.Lambda<Action<TSettings, TValue>>(setPropertyValue, parameter, propertyValue).Compile();
        Attribute = member.Member.GetCustomAttribute<SettingsEntryAttribute>();

        if (Attribute is { } attribute)
        {
            Header = attribute.LocalizedResourceHeader;
            Description = attribute.LocalizedResourceDescription;
            HeaderIcon = attribute.Symbol;
        }
    }

    public SettingsEntryAttribute? Attribute { get; }

    public Action<TValue>? ValueChanged { get; set; }

    public TValue Value
    {
        get => _getter(Settings);
        set
        {
            if (EqualityComparer<TValue>.Default.Equals(Value, value))
                return;
            _setter(Settings, value);
            OnPropertyChanged();
        }
    }

    public override void ValueReset()
    {
        OnPropertyChanged(nameof(Value));
        ValueChanged?.Invoke(Value);
    }

    private readonly Func<TSettings, TValue> _getter;

    private readonly Action<TSettings, TValue> _setter;
}
