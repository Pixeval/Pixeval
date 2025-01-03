// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Pixeval.Controls.Settings;
using Pixeval.Extensions.Common.Settings;

namespace Pixeval.Extensions.Models;

public partial class ExtensionColorSettingsEntry(IColorSettingsExtension extension, uint value, IPropertySet values) : ExtensionSettingsEntry<uint>(extension, value, values)
{
    public override FrameworkElement Element => new ColorSettingsCard { Entry = this };

    public override void ValueReset() => Value = extension.GetDefaultValue();

    public override void ValueSaving()
    {
        extension.OnValueChanged(Value);
        base.ValueSaving();
    }
}
