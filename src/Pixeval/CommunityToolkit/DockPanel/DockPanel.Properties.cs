#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/DockPanel.Properties.cs
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

namespace Pixeval.CommunityToolkit.DockPanel
{
    /// <summary>
    ///     Defines an area where you can arrange child elements either horizontally or vertically, relative to each other.
    /// </summary>
    public partial class DockPanel
    {
        /// <summary>
        ///     Gets or sets a value that indicates the position of a child element within a parent <see cref="DockPanel" />.
        /// </summary>
        public static readonly DependencyProperty DockProperty = DependencyProperty.RegisterAttached(
            "Dock",
            typeof(Dock),
            typeof(FrameworkElement),
            new PropertyMetadata(Dock.Left, DockChanged));

        /// <summary>
        ///     Identifies the <see cref="LastChildFill" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty LastChildFillProperty
            = DependencyProperty.Register(
                nameof(LastChildFill),
                typeof(bool),
                typeof(DockPanel),
                new PropertyMetadata(true, LastChildFillChanged));

        /// <summary>
        ///     Identifies the Padding dependency property.
        /// </summary>
        /// <returns>The identifier for the <see cref="Padding" /> dependency property.</returns>
        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register(
                nameof(Padding),
                typeof(Thickness),
                typeof(DockPanel),
                new PropertyMetadata(default(Thickness), OnPaddingChanged));

        /// <summary>
        ///     Gets or sets a value indicating whether the last child element within a DockPanel stretches to fill the remaining
        ///     available space.
        /// </summary>
        public bool LastChildFill
        {
            get => (bool) GetValue(LastChildFillProperty);
            set => SetValue(LastChildFillProperty, value);
        }

        /// <summary>
        ///     Gets or sets the distance between the border and its child object.
        /// </summary>
        /// <returns>
        ///     The dimensions of the space between the border and its child as a Thickness value.
        ///     Thickness is a structure that stores dimension values using pixel measures.
        /// </returns>
        public Thickness Padding
        {
            get => (Thickness) GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        /// <summary>
        ///     Gets DockProperty attached property
        /// </summary>
        /// <param name="obj">Target FrameworkElement</param>
        /// <returns>Dock value</returns>
        public static Dock GetDock(FrameworkElement obj)
        {
            return (Dock) obj.GetValue(DockProperty);
        }

        /// <summary>
        ///     Sets DockProperty attached property
        /// </summary>
        /// <param name="obj">Target FrameworkElement</param>
        /// <param name="value">Dock Value</param>
        public static void SetDock(FrameworkElement obj, Dock value)
        {
            obj.SetValue(DockProperty, value);
        }
    }
}