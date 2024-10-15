using FluentIcons.Common;
using Microsoft.UI.Xaml.Media;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<Symbol>("Symbol")]
[DependencyProperty<string>("Text")]
[DependencyProperty<Brush>("IconForeground")]
[DependencyProperty<Brush>("TextForeground")]
public sealed partial class IconText
{
    public IconText() => InitializeComponent();
}
