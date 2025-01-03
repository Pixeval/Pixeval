// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using Windows.Foundation.Collections;
using Pixeval.Attributes;
using Pixeval.Extensions.Common.Settings;
using Pixeval.Settings;

namespace Pixeval.Extensions;

public abstract class ExtensionSettingsEntry<TValue>(ISettingsExtension extension, TValue value, IPropertySet values)
    : ObservableSettingsEntryBase(extension.GetLabel(), extension.GetDescription(), extension.GetIcon()), ISingleValueSettingsEntry<TValue>
{
    public SettingsEntryAttribute? Attribute => null;

    public Action<TValue>? ValueChanged { get; set; }

    public TValue Value
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

    public override void ValueReset()
    {
    }

    public override void ValueSaving()
    {
        values[extension.GetToken()] = Value;
    }
}
