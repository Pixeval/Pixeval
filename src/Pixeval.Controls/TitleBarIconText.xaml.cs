using Microsoft.UI.Xaml.Controls;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<string>("Title")]
public sealed partial class TitleBarIconText : Grid
{
    public Image Icon => Image;

    public TextBlock TitleBlock => TextBlock;

    public TitleBarIconText() => InitializeComponent();
}
