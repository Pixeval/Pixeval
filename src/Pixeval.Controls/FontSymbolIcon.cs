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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Controls.MarkupExtensions;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<FontIconSymbol>("Symbol", DependencyPropertyDefaultValue.UnsetValue, nameof(PropertyChangedCallback))]
public partial class FontSymbolIcon : FontIcon
{
    private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.To<FontSymbolIcon>().Glyph = ((char)e.NewValue.To<FontIconSymbol>()).ToString();
    }
}

[DependencyProperty<FontIconSymbol>("Symbol", DependencyPropertyDefaultValue.UnsetValue, nameof(PropertyChangedCallback))]
public partial class FontSymbolIconSource : FontIconSource
{
    private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.To<FontSymbolIconSource>().Glyph = ((char)e.NewValue.To<FontIconSymbol>()).ToString();
    }
}
