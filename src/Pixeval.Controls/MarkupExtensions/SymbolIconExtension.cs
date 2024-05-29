#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2024 Pixeval.Controls/SymbolIconExtension.cs
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
using Microsoft.UI.Xaml.Markup;
using WinUI3Utilities.Controls;

namespace FluentIcons.WinUI;

/// <summary>
/// Symbol icon markup extension
/// </summary>
[MarkupExtensionReturnType(ReturnType = typeof(SymbolIcon))]
public class SymbolIconExtension : SymbolIconBaseExtension
{
    /// <inheritdoc cref="FrameworkElement.FlowDirection"/>
    public FlowDirection FlowDirection { get; set; }

    /// <inheritdoc />
    protected override object ProvideValue()
    {
        var icon = new SymbolIcon
        {
            IsFilled = IsFilled,
            Symbol = Symbol,
            FontWeight = FontWeight,
            FontStyle = FontStyle,
            IsTextScaleFactorEnabled = IsTextScaleFactorEnabled,
            MirroredWhenRightToLeft = MirroredWhenRightToLeft,
            FlowDirection = FlowDirection
        };

        if (Size is not FontSizeType.None)
            icon.FontSize = (int)Size;
        else if (FontSize > 0)
            icon.FontSize = FontSize;

        if (Foreground is not null)
            icon.Foreground = Foreground;

        return icon;
    }
}
