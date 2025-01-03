// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Pixeval.Controls.Settings;
using Pixeval.Extensions.Common.Settings;

namespace Pixeval.Extensions.Models;

public partial class ExtensionDateSettingsEntry(IDateTimeOffsetSettingsExtension extension, DateTimeOffset value, IPropertySet values) : ExtensionSettingsEntry<DateTimeOffset>(extension, value, values)
{
    public override FrameworkElement Element => new DateSettingsCard { Entry = this };

    public override void ValueReset() => Value = extension.GetDefaultValue();

    public override void ValueSaving()
    {
        extension.OnValueChanged(Value);
        base.ValueSaving();
    }
}
