#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2024 Pixeval.Controls/SymbolIconBaseExtension.cs
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

using Windows.UI.Text;
using FluentIcons.Common;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using WinUI3Utilities.Controls;

namespace FluentIcons.WinUI;

/// <summary>
/// An abstract <see cref="MarkupExtension"/> which to produce text-based icons.
/// </summary>
public abstract class SymbolIconBaseExtension : MarkupExtension
{
    /// <summary>
    /// Gets or sets if the icon is filled.
    /// </summary>
    public bool IsFilled { get; set; }

    /// <summary>
    /// Gets or sets the size of the icon to display.
    /// </summary>
    public double FontSize { get; set; }

    /// <summary>
    /// Gets or sets the symbol of the icon to display.
    /// </summary>
    public Symbol Symbol { get; set; }

    /// <summary>
    /// Gets or sets the size of the icon to display. Priority is higher than <see cref="FontSize"/>.
    /// </summary>
    public FontSizeType Size { get; set; }

    /// <summary>
    /// Gets or sets the thickness of the icon glyph.
    /// </summary>
    public FontWeight FontWeight { get; set; } = FontWeights.Normal;

    /// <summary>
    /// Gets or sets the font style for the icon glyph.
    /// </summary>
    public FontStyle FontStyle { get; set; } = FontStyle.Normal;

    /// <summary>
    /// Gets or sets the foreground <see cref="Brush"/> for the icon.
    /// </summary>
    public Brush? Foreground { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether automatic text enlargement, to reflect the system text size setting, is enabled.
    /// </summary>
    public bool IsTextScaleFactorEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the icon is mirrored when the flow direction is right to left.
    /// </summary>
    public bool MirroredWhenRightToLeft { get; set; }
}
