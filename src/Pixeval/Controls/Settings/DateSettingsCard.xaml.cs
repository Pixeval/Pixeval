// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Pixeval.Settings;

namespace Pixeval.Controls.Settings;

public sealed partial class DateSettingsCard
{
    public ISingleValueSettingsEntry<DateTimeOffset> Entry { get; set; } = null!;

    public DateSettingsCard() => InitializeComponent();
}
