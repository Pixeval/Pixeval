using Microsoft.UI.Xaml.Media;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<string>("Text")]
[DependencyProperty<Brush>("Fill")]
public sealed partial class DigitalSignalItem
{
    public DigitalSignalItem() => InitializeComponent();
}
