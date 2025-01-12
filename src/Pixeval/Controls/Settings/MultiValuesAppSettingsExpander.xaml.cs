// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Settings;

namespace Pixeval.Controls.Settings;

public sealed partial class MultiValuesAppSettingsExpander
{
    public MultiValuesEntry Entry
    {
        get;
        set
        {
            field = value;
            foreach (var entry in Entry.Entries)
                Items.Add(entry.Element);
        }
    } = null!;

    public MultiValuesAppSettingsExpander() => InitializeComponent();
}
