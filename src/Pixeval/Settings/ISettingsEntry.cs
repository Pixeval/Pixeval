// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using FluentIcons.Common;
using Microsoft.UI.Xaml;
using Windows.Foundation.Collections;

namespace Pixeval.Settings;

public interface ISettingsEntry
{
    FrameworkElement Element { get; }

    Symbol HeaderIcon { get; }

    string Header { get; }

    object DescriptionControl { get; }

    void ValueSaving(IPropertySet values);
}
