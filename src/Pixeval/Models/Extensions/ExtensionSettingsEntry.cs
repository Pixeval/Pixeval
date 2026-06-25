// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using AutoSettingsPage;
using AutoSettingsPage.Models;
using Pixeval.Controls;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.Settings;

namespace Pixeval.Models.Extensions;

public class ExtensionSettingsEntry<TExtension, TValue>(TExtension extension, TValue value, Func<TExtension, TValue> getDefaultValue, Action<TValue> onValueChanged)
    : ObservableSettingsEntry(extension.Token, extension.Label, extension.Description, extension.Icon), ISingleValueSettingsEntry<TValue>, IExtensionSettingEntry
    where TExtension : ISettingsExtension
{
    public event Action<TValue> ValueChanged = onValueChanged;

    public override Uri? DescriptionUri
    {
        get => extension.DescriptionUri is { } uri ? new Uri(uri) : null;
        set => throw new NotSupportedException();
    }

    public TValue Value
    {
        get;
        set
        {
            if (EqualityComparer<TValue>.Default.Equals(field, value))
                return;
            field = value;
            OnPropertyChanged();
            ValueChanged(value);
        }
    } = value;

    /// <inheritdoc />
    public string? Placeholder => extension.Placeholder;

    public void ValueReset()
    {
        Value = getDefaultValue(extension);
    }

    public void ValueSaving(Dictionary<string, object?> values)
    {
        ValueChanged(Value);
        values[Token] = Value;
    }
}

public class ExtensionDoubleSettingsEntry(IDoubleSettingsExtension extension, double value, Func<IDoubleSettingsExtension, double> getDefaultValue, Action<double> onValueChanged)
    : ExtensionSettingsEntry<IDoubleSettingsExtension, double>(extension, value, getDefaultValue, onValueChanged), INumberSettingsEntry<double>
{
    private readonly IDoubleSettingsExtension _extension = extension;

    /// <inheritdoc />
    public double Max => _extension.MaxValue;

    /// <inheritdoc />
    public double Min => _extension.MinValue;

    /// <inheritdoc />
    public double Step => _extension.StepValue;
}

public class ExtensionIntSettingsEntry(IIntSettingsExtension extension, int value, Func<IIntSettingsExtension, int> getDefaultValue, Action<int> onValueChanged)
    : ExtensionSettingsEntry<IIntSettingsExtension, int>(extension, value, getDefaultValue, onValueChanged), INumberSettingsEntry<int>
{
    private readonly IIntSettingsExtension _extension = extension;

    /// <inheritdoc />
    public int Max => _extension.MaxValue;

    /// <inheritdoc />
    public int Min => _extension.MinValue;

    /// <inheritdoc />
    public int Step => _extension.StepValue;
}

public class ExtensionEnumSettingsEntry(IEnumSettingsExtension extension, int value, Func<IEnumSettingsExtension, object> getDefaultValue, Action<int> onValueChanged)
    : ExtensionSettingsEntry<IEnumSettingsExtension, object>(extension, value, getDefaultValue, o => onValueChanged((int) o)), IEnumSettingsEntry<object>
{
    /// <inheritdoc />
    public IReadOnlyList<IReadOnlyStringPair<object>> EnumItems { get; } = [.. extension.EnumKeyValues.Select(t => new SymbolComboBoxItem(t.Value, t.Key, default))];
}
