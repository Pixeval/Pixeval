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

public partial class ExtensionStringsArraySettingsEntry(
    IStringsArraySettingsExtension extension,
    string[] value,
    IPropertySet values)
    : ExtensionSettingsEntry<ObservableCollection<string>>(extension, [.. value], values), IMultiStringsAppSettingsEntry
{
    public override FrameworkElement Element => new TokenizingSettingsExpander { Entry = this };

    public override void ValueReset() => Value = [..extension.GetDefaultValue()];

    public override void ValueSaving()
    {
        extension.OnValueChanged(Value.ToArray());
        base.ValueSaving();
    }

    public string? Placeholder => extension.GetPlaceholder();
}
