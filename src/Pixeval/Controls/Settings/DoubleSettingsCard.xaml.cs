// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Settings;

namespace Pixeval.Controls.Settings;

public sealed partial class DoubleSettingsCard
{
    public IDoubleSettingsEntry Entry { get; set; } = null!;

    public DoubleSettingsCard() => InitializeComponent();
}
