#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/TextIconExtension.cs
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

using System;
using Windows.UI.Text;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;

namespace Pixeval.CommunityToolkit
{
    /// <summary>
    ///     An abstract <see cref="MarkupExtension" /> which to produce text-based icons.
    /// </summary>
    public abstract class TextIconExtension : MarkupExtension
    {
        [ThreadStatic]
        private static FontFamily? segoeMDL2AssetsFontFamily;

        /// <summary>
        ///     Gets or sets the size of the icon to display.
        /// </summary>
        public double FontSize { get; set; }

        /// <summary>
        ///     Gets the reusable "Segoe MDL2 Assets" <see cref="FontFamily" /> instance.
        /// </summary>
        protected static FontFamily SegoeMDL2AssetsFontFamily => segoeMDL2AssetsFontFamily ??= new FontFamily("Segoe MDL2 Assets");

        /// <summary>
        ///     Gets or sets the thickness of the icon glyph.
        /// </summary>
        public FontWeight FontWeight { get; set; } = FontWeights.Normal;

        /// <summary>
        ///     Gets or sets the font style for the icon glyph.
        /// </summary>
        public FontStyle FontStyle { get; set; } = FontStyle.Normal;

        /// <summary>
        ///     Gets or sets the foreground <see cref="Brush" /> for the icon.
        /// </summary>
        public Brush? Foreground { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether automatic text enlargement, to reflect the system text size setting, is
        ///     enabled.
        /// </summary>
        public bool IsTextScaleFactorEnabled { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the icon is mirrored when the flow direction is right to left.
        /// </summary>
        public bool MirroredWhenRightToLeft { get; set; }
    }
}