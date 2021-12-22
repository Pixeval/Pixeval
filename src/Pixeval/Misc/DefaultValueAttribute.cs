#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/DefaultValueAttribute.cs
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
using System.Linq;
using System.Reflection;

namespace Pixeval.Misc;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class DefaultValueAttribute : Attribute
{
    public DefaultValueAttribute(object? value)
    {
        Value = value;
    }

    /// <summary>
    ///     Create a <see cref="DefaultValueAttribute" /> using a factory type, the type will be instantiated to provide the value
    /// </summary>
    /// <param name="valueFactoryType"></param>
    public DefaultValueAttribute(Type? valueFactoryType)
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
        if (propInfo.GetCustomAttribute<DefaultValueAttribute>() is { } attribute)
        {
            if (attribute.ValueFactoryType is { } type)
            {
                return type.IsAssignableTo(typeof(IDefaultValueProvider))
                    ? ((IDefaultValueProvider) Activator.CreateInstance(type)!).ProvideValue()
                    : Activator.CreateInstance(type);
            }

            return attribute.Value;
        }
        //TODO 为什么要设为null？
        return null;
    }

    public static void Initialize(object obj)
    {
        foreach (var propertyInfo in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            propertyInfo.SetValue(obj, propertyInfo.GetDefaultValue());
        }
        foreach (var fieldInfo in obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
        {
            var fieldName = fieldInfo.Name.TrimStart('_');

            if (obj.GetType().GetProperty(fieldName[..1].ToUpperInvariant() + fieldName[1..]) is { } propertyInfo)
                propertyInfo.SetValue(obj, propertyInfo.GetDefaultValue());
        }
    }

}