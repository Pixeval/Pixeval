using WinUI3Utilities.Attributes;
using WinUI3Utilities.Controls;

namespace Pixeval.Controls;

[DependencyProperty<IconGlyph>("Glyph")]
[DependencyProperty<string>("Text")]
public sealed partial class IconText
{
    public IconText() => InitializeComponent();
}
