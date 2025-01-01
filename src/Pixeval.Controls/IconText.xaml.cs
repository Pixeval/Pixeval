using FluentIcons.Common;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<Symbol>("Symbol")]
[DependencyProperty<string>("Text")]
public sealed partial class IconText
{
    public IconText() => InitializeComponent();
}
