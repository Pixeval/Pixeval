using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using Symbol = FluentIcons.Common.Symbol;

namespace Pixeval.Controls;

public sealed partial class IconText : Control
{
    [GeneratedDependencyProperty]
    public partial Symbol Symbol { get; set; }

    [GeneratedDependencyProperty]
    public partial string? Text { get; set; }

    public IconText() => DefaultStyleKey = typeof(IconText);
}
