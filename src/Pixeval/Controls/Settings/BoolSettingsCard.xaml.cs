using Pixeval.Settings;

namespace Pixeval.Controls.Settings;

public sealed partial class BoolSettingsCard
{
    public ISingleValueSettingsEntry<bool> Entry { get; set; } = null!;

    public BoolSettingsCard() => InitializeComponent();
}
