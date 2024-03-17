using Microsoft.UI.Xaml.Controls;
using WinUI3Utilities;

namespace Pixeval.Controls;

public sealed partial class ColorPickerButton : UserControl
{
    private ColorPicker _colorPicker = null!;

    public ColorPickerButton() => InitializeComponent();

    public ColorPicker ColorPicker
    {
        get => _colorPicker;
        set => Content.To<Button>().Flyout.To<Flyout>().Content = _colorPicker = value;
    }
}
