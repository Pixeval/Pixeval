// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Pixeval.Extensions.Common.Settings;

namespace Pixeval.Extensions.Models;

public partial class ExtensionStringsArraySettingsEntry(IStringsArraySettingsExtension extension, string[] value, IPropertySet values) : ExtensionSettingsEntry<string[]>(extension, value, null!)
{
    public override FrameworkElement Element => throw new NotImplementedException();

    public override void ValueSaving()
    {
        extension.OnValueChanged(Value);
        values[extension.GetToken()] = SettingsValueConverter.Convert(Value);
    }
}
