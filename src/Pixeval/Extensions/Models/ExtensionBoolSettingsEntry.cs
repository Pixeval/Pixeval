// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml;
using Pixeval.Controls.Settings;
using Pixeval.Extensions.Common.Settings;
using Windows.Foundation.Collections;

namespace Pixeval.Extensions.Models;

public partial class ExtensionBoolSettingsEntry(IBoolSettingsExtension extension, bool value)
    : ExtensionSettingsEntry<bool>(extension, value)
{
    public override FrameworkElement Element => new BoolSettingsCard { Entry = this };

    public override void ValueReset() => Value = extension.GetDefaultValue();

    public override void ValueSaving(IPropertySet values)
    {
        extension.OnValueChanged(Value);
        base.ValueSaving(values);
    }
}
