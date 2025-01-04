// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.ObjectModel;
using System.Linq;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Pixeval.Controls.Settings;
using Pixeval.Extensions.Common.Settings;
using Pixeval.Settings;

namespace Pixeval.Extensions.Models;

public partial class ExtensionStringsArraySettingsEntry(IStringsArraySettingsExtension extension, string[] value)
    : ExtensionSettingsEntry<ObservableCollection<string>>(extension, [.. value]), IMultiStringsAppSettingsEntry
{
    public override FrameworkElement Element => new TokenizingSettingsExpander { Entry = this };

    public override void ValueReset() => Value = [..extension.GetDefaultValue()];

    public override void ValueSaving(IPropertySet values)
    {
        extension.OnValueChanged(Value.ToArray());
        base.ValueSaving(values);
    }

    public string? Placeholder => extension.GetPlaceholder();
}
