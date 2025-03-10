// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Settings;
using Windows.UI;

namespace Pixeval.Controls.Settings;

public sealed partial class ColorSettingsCard
{
    public ISingleValueSettingsEntry<uint> Entry { get; set; } = null!;

    public ColorSettingsCard() => InitializeComponent();

    private void ColorBindBack(Color color) => Entry.Value = C.ToAlphaUInt(color);
}
