#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/SettingsViewModelAttribute.cs
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
/// 根据设置类属性生成ViewModel属性（可用<see cref="SettingsViewModelExclusionAttribute"/>类，在设置类里指定不生成属性的例外）
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class SettingsViewModelAttribute : Attribute
{
    public SettingsViewModelAttribute(Type settingType, string settingName)
    {
        SettingType = settingType;
        SettingName = settingName;
    }
    public Type SettingType { get; }

    public string SettingName { get; }
}