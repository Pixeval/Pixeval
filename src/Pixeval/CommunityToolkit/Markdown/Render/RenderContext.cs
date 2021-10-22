#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/RenderContext.cs
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

using Microsoft.UI.Xaml.Media;
using Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Render;

namespace Pixeval.CommunityToolkit.Markdown.Render
{
    /// <summary>
    ///     The Context of the Current Position
    /// </summary>
    public abstract class RenderContext : IRenderContext
    {
        /// <summary>
        ///     Gets or sets the Foreground of the Current Context.
        /// </summary>
        public Brush? Foreground { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to override the Foreground Property.
        /// </summary>
        public bool OverrideForeground { get; set; }

        /// <inheritdoc />
        public bool TrimLeadingWhitespace { get; set; }

        /// <inheritdoc />
        public object? Parent { get; set; }

        /// <inheritdoc />
        public IRenderContext Clone()
        {
            return (IRenderContext) MemberwiseClone();
        }
    }
}