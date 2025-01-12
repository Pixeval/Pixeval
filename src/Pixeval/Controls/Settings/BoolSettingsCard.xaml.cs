// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Settings;

namespace Pixeval.Controls.Settings;

public sealed partial class BoolSettingsCard
{
    public ISingleValueSettingsEntry<bool> Entry { get; set; } = null!;

    public BoolSettingsCard() => InitializeComponent();
}
