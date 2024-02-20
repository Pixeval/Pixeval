using Microsoft.UI.Xaml.Controls;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<string>("Title")]
[DependencyProperty<int>("Number")]
[DependencyProperty<IconElement>("Icon")]
public sealed partial class AppButtonItem
{
    public AppButtonItem() => InitializeComponent();
}
