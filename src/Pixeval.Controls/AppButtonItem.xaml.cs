using Pixeval.Controls.MarkupExtensions;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<string>("Title")]
[DependencyProperty<int>("Number")]
[DependencyProperty<FontIconSymbol>("Symbol")]
public sealed partial class AppButtonItem
{
    public AppButtonItem() => InitializeComponent();
}
