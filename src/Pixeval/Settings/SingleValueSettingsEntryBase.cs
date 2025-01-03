using System;
using Windows.Foundation.Collections;
using FluentIcons.Common;
using WinUI3Utilities;

namespace Pixeval.Settings;

public abstract class SingleValueSettingsEntryBase<TValue>(
    string header,
    string description,
    Symbol headerIcon,
    IPropertySet values)
    : ObservableSettingsEntryBase(header, description, headerIcon)
{
    public abstract TValue Value { get; set; }

    public abstract string Token { get; }

    public virtual ISettingsValueConverter Converter { get; } = new SettingsValueConverter();

    public override void ValueSaving()
    {
        values[Token] = Converter.Convert(Value);
    }
}
