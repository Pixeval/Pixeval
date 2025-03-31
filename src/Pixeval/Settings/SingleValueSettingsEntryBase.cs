// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using FluentIcons.Common;
using Windows.Foundation.Collections;
using WinUI3Utilities;

namespace Pixeval.Settings;

public abstract class SingleValueSettingsEntryBase<TValue>(
    string header,
    string description,
    Symbol headerIcon)
    : ObservableSettingsEntryBase(header, description, headerIcon)
{
    public abstract TValue Value { get; set; }

    public abstract string Token { get; }

    public virtual ISettingsValueConverter Converter { get; } = new SettingsValueConverter();

    public override void ValueSaving(IPropertySet values)
    {
        values[Token] = Converter.Convert(Value);
    }
}
