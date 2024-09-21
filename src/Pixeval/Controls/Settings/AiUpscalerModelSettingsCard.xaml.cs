

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using Microsoft.UI.Xaml.Controls;
using Pixeval.Settings.Models;

namespace Pixeval.Controls.Settings
{
    public sealed partial class AiUpscalerModelSettingsCard
    {
        public AiUpscalerModelSettingsCard()
        {
            InitializeComponent();
        }

        public AiUpscalerModelSettingsEntry Entry { get; set; } = null!;

        private void EnumComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            Entry.ValueChanged?.Invoke(Entry.Value);
        }
    }
}
