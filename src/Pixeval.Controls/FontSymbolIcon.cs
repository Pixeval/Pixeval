#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/FontSymbolIcon.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Pixeval.Controls.MarkupExtensions;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<FontIconSymbol>("Symbol", DependencyPropertyDefaultValue.UnsetValue, nameof(SymbolPropertyChangedCallback))]
[DependencyProperty<Size>("Size", DependencyPropertyDefaultValue.UnsetValue, nameof(SizePropertyChangedCallback))]
[DependencyProperty<bool>("IsBackLayer", DependencyPropertyDefaultValue.UnsetValue, nameof(IsBackLayerPropertyChangedCallback))]
public partial class FontSymbolIcon : FontIcon
{
    private static void SymbolPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.To<FontSymbolIcon>().Glyph = ((char)e.NewValue.To<FontIconSymbol>()).ToString();
    }

    private static void SizePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is { } size and not Size.None)
            d.To<FontSymbolIcon>().FontSize = (int)size;
    }

    private static void IsBackLayerPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
            d.To<FontSymbolIcon>().Foreground = new SolidColorBrush(Color.FromArgb(0x80, 0, 0, 0));
    }
}

[DependencyProperty<FontIconSymbol>("Symbol", DependencyPropertyDefaultValue.UnsetValue, nameof(SymbolPropertyChangedCallback))]
[DependencyProperty<Size>("Size", DependencyPropertyDefaultValue.UnsetValue, nameof(SizePropertyChangedCallback))]
[DependencyProperty<bool>("IsBackLayer", DependencyPropertyDefaultValue.UnsetValue, nameof(IsBackLayerPropertyChangedCallback))]
public partial class FontSymbolIconSource : FontIconSource
{
    private static void SymbolPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.To<FontSymbolIcon>().Glyph = ((char)e.NewValue.To<FontIconSymbol>()).ToString();
    }

    private static void SizePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is { } size and not Size.None)
            d.To<FontSymbolIcon>().FontSize = (int)size;
    }

    private static void IsBackLayerPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
            d.To<FontSymbolIcon>().Foreground = new SolidColorBrush(Color.FromArgb(0x80, 0, 0, 0));
    }
}
