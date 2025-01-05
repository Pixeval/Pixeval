using Pixeval.Settings.Models;

namespace Pixeval.Controls.Settings;

public sealed partial class DateWithSwitchSettingsCard
{
    public DateWithSwitchAppSettingsEntry Entry { get; set; } = null!;

    public DateWithSwitchSettingsCard() => InitializeComponent();
}
