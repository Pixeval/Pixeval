// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using Pixeval.Attributes;
using Pixeval.Extensions.Common.Settings;
using Pixeval.Extensions.Models;
using Pixeval.Settings;
using WinUI3Utilities;

namespace Pixeval.Extensions;

public abstract class ExtensionSettingsEntry<TValue>(ISettingsExtension extension, TValue value)
    : SingleValueSettingsEntryBase<TValue>(extension.GetLabel(), extension.GetDescription(), extension.GetIcon()), ISingleValueSettingsEntry<TValue>, IExtensionSettingEntry
{
    public SettingsEntryAttribute? Attribute => null;

    public override string Token => extension.GetToken();

    public Action<TValue>? ValueChanged { get; set; }

    public override Uri? DescriptionUri
    {
        get => extension.GetDescriptionUri() is { } uri ? new Uri(uri) : null;
        set => ThrowHelper.NotSupported<Uri?>();
    }

    public override TValue Value
    {
        get;
        set
        {
            if (EqualityComparer<TValue>.Default.Equals(field, value))
                return;
            field = value;
            OnPropertyChanged();
            ValueChanged?.Invoke(Value);
        }
    } = value;

    public abstract void ValueReset();
}
