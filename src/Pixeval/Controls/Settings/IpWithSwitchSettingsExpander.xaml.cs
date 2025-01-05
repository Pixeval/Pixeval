using Pixeval.Settings.Models;

namespace Pixeval.Controls.Settings;

public sealed partial class IpWithSwitchSettingsExpander
{
    public IpWithSwitchAppSettingsEntry Entry { get; set; } = null!;

    public IpWithSwitchSettingsExpander() => InitializeComponent();
}
