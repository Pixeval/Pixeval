using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Attributes;
using Pixeval.Util.UI;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.UserControls;

[DependencyProperty<FontIconSymbols>("Symbol", DependencyPropertyDefaultValue.UnsetValue, nameof(PropertyChangedCallback))]
public partial class FontSymbolIcon : FontIcon
{
    private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.To<FontIcon>().Glyph = e.NewValue.To<FontIconSymbols>().GetMetadataOnEnumMember();
    }
}
