// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml;
using Pixeval.Controls.Settings;
using Pixeval.Extensions.Common.Settings;
using Pixeval.Settings;
using Windows.Foundation.Collections;

namespace Pixeval.Extensions.Models;

public partial class ExtensionDoubleSettingsEntry(IDoubleSettingsExtension extension, double value)
    : ExtensionSettingsEntry<double>(extension, value), IDoubleSettingsEntry
{
    public override FrameworkElement Element => new DoubleSettingsCard { Entry = this };

    public override void ValueReset() => Value = extension.GetDefaultValue();

    public override void ValueSaving(IPropertySet values)
    {
        extension.OnValueChanged(Value);
        base.ValueSaving(values);
    }

    public string? Placeholder => extension.GetPlaceholder();

    public double Max => extension.GetMaxValue();

    public double Min => extension.GetMinValue();

    public double LargeChange => extension.GetLargeChange();

    public double SmallChange => extension.GetSmallChange();
}
