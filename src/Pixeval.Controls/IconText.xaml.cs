using Pixeval.Controls.MarkupExtensions;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<FontIconSymbol>("Symbol")]
[DependencyProperty<string>("Text")]
public sealed partial class IconText
{
    public IconText() => InitializeComponent();
}
