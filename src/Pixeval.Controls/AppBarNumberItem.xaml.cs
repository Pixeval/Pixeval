using FluentIcons.Common;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<string>("Title")]
[DependencyProperty<int>("Number")]
[DependencyProperty<Symbol>("Symbol")]
public sealed partial class AppBarNumberItem
{
    public AppBarNumberItem() => InitializeComponent();
}
