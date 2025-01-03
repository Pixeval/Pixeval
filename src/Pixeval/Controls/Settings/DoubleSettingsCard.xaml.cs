using Pixeval.Settings;

namespace Pixeval.Controls.Settings;

public sealed partial class DoubleSettingsCard
{
    public IDoubleSettingsEntry Entry { get; set; } = null!;

    public DoubleSettingsCard() => InitializeComponent();
}
