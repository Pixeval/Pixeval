using Pixeval.Settings;

namespace Pixeval.Controls.Settings;

public sealed partial class StringSettingsCard
{
    public IStringSettingsEntry Entry { get; set; } = null!;

    public StringSettingsCard() => InitializeComponent();
}
