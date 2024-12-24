using FluentIcons.Common;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<string>("Text")]
[DependencyProperty<Symbol>("Symbol")]
public sealed partial class AppBarTextItem
{
    public AppBarTextItem() => InitializeComponent();
}
