#region Copyright (c) Pixeval/Pixeval.SourceGen

// GPL v3 License
// 
// Pixeval/Pixeval.SourceGen
// Copyright (c) 2021 Pixeval.SourceGen/DependencyPropertyGenerator.cs
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

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class LoadSaveConfigurationAttribute : Attribute
{
    public LoadSaveConfigurationAttribute(Type targetType, string containerName)
    {
        TargetType = targetType;
        ContainerName = containerName;
    }
    public Type TargetType { get; }
    public string ContainerName { get; }
    public string CastMethod { get; set; } = "null!";
}