using System;
using Pixeval.Settings;

namespace Pixeval.Controls.Settings;

public sealed partial class DateSettingsCard
{
    public ISingleValueSettingsEntry<DateTimeOffset> Entry { get; set; } = null!;

    public DateSettingsCard() => InitializeComponent();
}
