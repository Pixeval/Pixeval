using Pixeval.Settings;

namespace Pixeval.Controls.Settings;

public sealed partial class EnumSettingsCard
{
    public IEnumSettingsEntry Entry { get; set; } = null!;

    public EnumSettingsCard() => InitializeComponent();
}
