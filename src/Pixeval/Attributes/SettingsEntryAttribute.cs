// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

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
public class SettingsEntryAttribute(Symbol symbol, string resourceKeyHeader, string? resourceKeyDescription) : Attribute
{
    public SettingsEntryAttribute(int symbol, string resourceKeyHeader, string? resourceKeyDescription)
        : this((Symbol) symbol, resourceKeyHeader, resourceKeyDescription)
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
