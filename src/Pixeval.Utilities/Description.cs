﻿#region Copyright (c) Pixeval/Pixeval.Utilities
// GPL v3 License
// 
// Pixeval/Pixeval.Utilities
// Copyright (c) 2021 Pixeval.Utilities/Description.cs
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

namespace Pixeval.Utilities;

[AttributeUsage(AttributeTargets.Field)]
public class Description : Attribute
{
    public Description(string name)
    {
        Name = name;
    }

    public string Name { get; }
}

public static class DescriptionHelper
{
    public static string GetDescription<TEnum>(this TEnum @enum) where TEnum : Enum
    {
        return (typeof(TEnum).GetField(@enum.ToString())?.GetCustomAttribute(typeof(Description)) as Description)?.Name ?? throw new InvalidOperationException("Attribute not found");
    }
}