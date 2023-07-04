#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/DefaultValueAttribute.cs
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
using System.Reflection;
using Pixeval.Misc;


namespace Pixeval.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class DefaultValue : Attribute
{
    public DefaultValue(object? value)
    {
        Value = value;
    }

    /// <summary>
    ///     Create a <see cref="DefaultValue" /> using a factory type, the type will be instantiated to provide the value
    /// </summary>
    /// <param name="valueFactoryType"></param>
    public DefaultValue(Type? valueFactoryType)
    {
        ValueFactoryType = valueFactoryType;
    }

    public object? Value { get; }

    public Type? ValueFactoryType { get; }
}

public static class DefaultValueAttributeHelper
{
    public static object? GetDefaultValue(this PropertyInfo propInfo)
    {
        if (propInfo.GetCustomAttribute<DefaultValue>() is { } attribute)
        {
            if (attribute.ValueFactoryType is { } type)
            {
                return type.IsAssignableTo(typeof(IDefaultValueProvider))
                    ? ((IDefaultValueProvider)Activator.CreateInstance(type)!).ProvideValue()
                    : Activator.CreateInstance(type);
            }

            return attribute.Value;
        }

        return null;
    }

    public static void Initialize(object obj)
    {
        foreach (var propertyInfo in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            propertyInfo.SetValue(obj, propertyInfo.GetDefaultValue());
        }
    }
}