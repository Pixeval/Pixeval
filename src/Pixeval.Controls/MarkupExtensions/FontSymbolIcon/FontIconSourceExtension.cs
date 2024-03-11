#region Copyright (c) Pixeval/Pixeval.Controls
// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2023 Pixeval.Controls/FontIconSourceExtension.cs
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

using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using WinUI3Utilities;

namespace Pixeval.Controls.MarkupExtensions;

[MarkupExtensionReturnType(ReturnType = typeof(FontIconSource))]
public class FontSymbolIconSourceExtension : TextIconExtension
{
    public FontIconSymbol Glyph { get; set; }

    public FontFamily? FontFamily { get; set; }

    public Size Size { get; set; }

    /// <inheritdoc />
    protected override object ProvideValue()
    {
        var icon = new FontIconSource
        {
            Glyph = ((char)Glyph).ToString(),
            FontFamily = FontFamily ??
                         new FontFamily(AppHelper.IsWindows11 ? "Segoe Fluent Icons" : "Segoe MDL2 Assets"),
            FontWeight = FontWeight,
            FontStyle = FontStyle,
            IsTextScaleFactorEnabled = IsTextScaleFactorEnabled,
            MirroredWhenRightToLeft = MirroredWhenRightToLeft
        };

        if (Size is not Size.None)
            icon.FontSize = (int)Size;
        else if (FontSize > 0)
            icon.FontSize = FontSize;

        if (Foreground is not null)
            icon.Foreground = Foreground;

        return icon;
    }
}
