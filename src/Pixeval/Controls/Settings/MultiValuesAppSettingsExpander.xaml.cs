// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml;
using Pixeval.Settings;

namespace Pixeval.Controls.Settings;

public sealed partial class MultiValuesAppSettingsExpander
{
    public MultiValuesEntry Entry { get; set; } = null!;

    public MultiValuesAppSettingsExpander() => InitializeComponent();

    private void MultiValuesAppSettingsExpander_OnLoaded(object sender, RoutedEventArgs e)
    {
        foreach (var entry in Entry.Entries)
            Items.Add(entry.Element);
    }
}
