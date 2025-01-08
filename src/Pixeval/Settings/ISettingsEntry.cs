// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Windows.Foundation.Collections;
using FluentIcons.Common;
using Microsoft.UI.Xaml;

namespace Pixeval.Settings;

public interface ISettingsEntry
{
    FrameworkElement Element { get; }

    Symbol HeaderIcon { get; } 

    string Header { get; }

    object DescriptionControl { get; }

    void ValueSaving(IPropertySet values);
}
