#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/DependencyPropertyAttribute.cs
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

using Microsoft.UI.Xaml;
using System;

namespace Pixeval.Attributes;

/// <summary>
///     生成如下代码
///     <code>
/// <see langword="public static readonly"/> <see cref="DependencyProperty"/> Property = <see cref="DependencyProperty"/>.Register("Field", <see langword="typeof"/>(Type), <see langword="typeof"/>(TClass), <see langword="new"/> <see cref="PropertyMetadata"/>(DefaultValue, OnPropertyChanged));
/// <br/>
/// <see langword="public"/> <see cref="T:Pixeval.Attributes.DependencyPropertyAttribute`1"/> Field { <see langword="get"/> => (<see cref="T:Pixeval.Attributes.DependencyPropertyAttribute`1"/>)GetValue(Property); <see langword="set"/> => SetValue(Property, <see langword="value"/>); }
///     </code>
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class DependencyPropertyAttribute<T> : Attribute
{
    public DependencyPropertyAttribute(string name, string propertyChanged = "")
    {
        Name = name;
        PropertyChanged = propertyChanged;
    }

    public string Name { get; }

    public string PropertyChanged { get; }

    public bool IsSetterPublic { get; init; } = true;

    public bool IsNullable { get; init; } = true;

    public string DefaultValue { get; init; } = "DependencyProperty.UnsetValue";
}
