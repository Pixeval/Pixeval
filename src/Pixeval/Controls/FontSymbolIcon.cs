using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Controls.MarkupExtensions;
using Pixeval.Controls.MarkupExtensions.FontSymbolIcon;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<FontIconSymbols>("Symbol", DependencyPropertyDefaultValue.UnsetValue, nameof(PropertyChangedCallback))]
public partial class FontSymbolIcon : FontIcon
{
    private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.To<FontIcon>().Glyph = e.NewValue.To<FontIconSymbols>().GetGlyph().ToString();
    }
}
