#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/SettingEntry.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentIcons.Common;
using Pixeval.AppManagement;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public partial class SettingsEntryAttribute(Symbol symbol, string resourceKeyHeader, string? resourceKeyDescription) : Attribute
{
    public SettingsEntryAttribute(int symbol, string resourceKeyHeader, string? resourceKeyDescription)
        : this((Symbol)symbol, resourceKeyHeader, resourceKeyDescription)
    {
    }

    public static readonly SettingsEntryAttribute AutoUpdate = new(Symbol.Communication,
        nameof(SettingsPageResources.DownloadUpdateAutomaticallyEntryHeader),
        nameof(SettingsPageResources.DownloadUpdateAutomaticallyEntryDescription));

    public static readonly SettingsEntryAttribute SignOut = new(Symbol.SignOut,
        nameof(SettingsPageResources.SignOutEntryHeader),
        nameof(SettingsPageResources.SignOutEntryDescription));

    public static readonly SettingsEntryAttribute ResetSettings = new(Symbol.Apps,
        nameof(SettingsPageResources.ResetDefaultSettingsEntryHeader),
        nameof(SettingsPageResources.ResetDefaultSettingsEntryDescription));

    public static readonly SettingsEntryAttribute DeleteHistories = new(Symbol.Delete,
        nameof(SettingsPageResources.DeleteHistoriesEntryHeader),
        null);

    public static readonly Lazy<IEnumerable<SettingsEntryAttribute>> LazyValues = new(() =>
        typeof(AppSettings).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .SelectNotNull(f => f.GetCustomAttribute<SettingsEntryAttribute>()));

    public Symbol Symbol { get; } = symbol;

    public string LocalizedResourceHeader { get; } =
        SettingsPageResources.GetResource(resourceKeyHeader);

    public string LocalizedResourceDescription { get; } =
        resourceKeyDescription is null
            ? ""
            : SettingsPageResources.GetResource(resourceKeyDescription);

    public static SettingsEntryAttribute GetFromPropertyName(string propertyName)
    {
        return GetFromPropertyName<AppSettings>(propertyName);
    }

    public static SettingsEntryAttribute GetFromPropertyName<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(string propertyName)
    {
        return typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)
                   ?.GetCustomAttribute<SettingsEntryAttribute>() ??
               ThrowHelper.Argument<string, SettingsEntryAttribute>(propertyName);
    }
}
