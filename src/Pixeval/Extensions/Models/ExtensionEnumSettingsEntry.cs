// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Pixeval.Controls;
using Pixeval.Controls.Settings;
using Pixeval.Extensions.Common.Settings;
using Pixeval.Settings;

namespace Pixeval.Extensions.Models;

public partial class ExtensionEnumSettingsEntry(IEnumSettingsExtension extension, int value)
    : ExtensionSettingsEntry<object>(extension, value), IEnumSettingsEntry
{
    public override FrameworkElement Element => new EnumSettingsCard { Entry = this };

    public override void ValueReset() => Value = extension.GetDefaultValue();

    public override void ValueSaving(IPropertySet values)
    {
        extension.OnValueChanged((int)Value);
        base.ValueSaving(values);
    }

    public IReadOnlyList<StringRepresentableItem> EnumItems => extension.GetEnumKeyValues()
        .Select(t => new StringRepresentableItem(t.Value, t.Key)).ToArray();
}
