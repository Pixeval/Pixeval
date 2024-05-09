using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Settings.Models;
using WinUI3Utilities;
using ColorPicker = Microsoft.UI.Xaml.Controls.ColorPicker;

namespace Pixeval.Controls.Settings;

public sealed partial class ColorSettingsCard
{
    public ColorAppSettingsEntry Entry { get; set; } = null!;

    public ColorSettingsCard() => InitializeComponent();

    private void ColorPicker_OnLoaded(object sender, RoutedEventArgs e)
    {
        sender.To<ColorPickerButton>().ColorPicker.ColorChanged += ColorPicker_OnColorChanged;
    }

    private void ColorPicker_OnColorChanged(ColorPicker sender, ColorChangedEventArgs args)
    {
        Entry.ValueChanged?.Invoke(Entry.Value);
    }
}
