#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/DependencyPropertyAttribute.cs
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

namespace Pixeval.Attributes;

/// <summary>
///     生成如下代码
///     <code>
/// public static readonly DependencyProperty Property = DependencyProperty.Register("Field", typeof(Type), typeof(TClass), new PropertyMetadata(DefaultValue, OnPropertyChanged));
/// public Type Field { get => (Type)GetValue(Property); set => SetValue(Property, value); }
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class DependencyPropertyAttribute : Attribute
{
    public DependencyPropertyAttribute(string name, Type type, string propertyChanged = "")
    {
        Name = name;
        Type = type;
        PropertyChanged = propertyChanged;
        IsSetterPublic = true;
        IsNullable = true;
    }

    public string Name { get; }

    public Type Type { get; }

    public string PropertyChanged { get; init; }

    public bool IsSetterPublic { get; init; }

    public bool IsNullable { get; init; }

    public string DefaultValue { get; init; } = "DependencyProperty.UnsetValue";
}