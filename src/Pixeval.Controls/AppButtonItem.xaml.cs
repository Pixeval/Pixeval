using WinUI3Utilities.Attributes;
using WinUI3Utilities.Controls;

namespace Pixeval.Controls;

[DependencyProperty<string>("Title")]
[DependencyProperty<int>("Number")]
[DependencyProperty<IconGlyph>("Glyph")]
public sealed partial class AppButtonItem
{
    public AppButtonItem() => InitializeComponent();
}
