#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/DockPanel.cs
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
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pixeval.CommunityToolkit.DockPanel
{
    /// <summary>
    ///     Defines an area where you can arrange child elements either horizontally or vertically, relative to each other.
    /// </summary>
    public partial class DockPanel : Panel
    {
        private static void DockChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var senderElement = sender as FrameworkElement;
            var dockPanel = senderElement?.FindParent<DockPanel>();

            dockPanel?.InvalidateArrange();
        }

        private static void LastChildFillChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var dockPanel = (DockPanel) sender;
            dockPanel.InvalidateArrange();
        }

        private static void OnPaddingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var dockPanel = (DockPanel) sender;
            dockPanel.InvalidateMeasure();
        }

        /// <inheritdoc />
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count == 0)
            {
                return finalSize;
            }

            var currentBounds = new Rect(Padding.Left, Padding.Top, finalSize.Width - Padding.Right, finalSize.Height - Padding.Bottom);
            var childrenCount = LastChildFill ? Children.Count - 1 : Children.Count;

            for (var index = 0; index < childrenCount; index++)
            {
                var child = Children[index];
                var dock = (Dock) child.GetValue(DockProperty);
                double width, height;
                switch (dock)
                {
                    case Dock.Left:

                        width = Math.Min(child.DesiredSize.Width, GetPositiveOrZero(currentBounds.Width - currentBounds.X));
                        child.Arrange(new Rect(currentBounds.X, currentBounds.Y, width, GetPositiveOrZero(currentBounds.Height - currentBounds.Y)));
                        currentBounds.X += width;

                        break;
                    case Dock.Top:

                        height = Math.Min(child.DesiredSize.Height, GetPositiveOrZero(currentBounds.Height - currentBounds.Y));
                        child.Arrange(new Rect(currentBounds.X, currentBounds.Y, GetPositiveOrZero(currentBounds.Width - currentBounds.X), height));
                        currentBounds.Y += height;

                        break;
                    case Dock.Right:

                        width = Math.Min(child.DesiredSize.Width, GetPositiveOrZero(currentBounds.Width - currentBounds.X));
                        child.Arrange(new Rect(GetPositiveOrZero(currentBounds.Width - width), currentBounds.Y, width, GetPositiveOrZero(currentBounds.Height - currentBounds.Y)));
                        currentBounds.Width -= currentBounds.Width - width > 0 ? width : 0;

                        break;
                    case Dock.Bottom:

                        height = Math.Min(child.DesiredSize.Height, GetPositiveOrZero(currentBounds.Height - currentBounds.Y));
                        child.Arrange(new Rect(currentBounds.X, GetPositiveOrZero(currentBounds.Height - height), GetPositiveOrZero(currentBounds.Width - currentBounds.X), height));
                        currentBounds.Height -= currentBounds.Height - height > 0 ? height : 0;

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (LastChildFill)
            {
                var width = GetPositiveOrZero(currentBounds.Width - currentBounds.X);
                var height = GetPositiveOrZero(currentBounds.Height - currentBounds.Y);
                var child = Children[^1];
                child.Arrange(
                    new Rect(currentBounds.X, currentBounds.Y, width, height));
            }

            return finalSize;
        }

        /// <inheritdoc />
        protected override Size MeasureOverride(Size availableSize)
        {
            var parentWidth = 0.0;
            var parentHeight = 0.0;
            var accumulatedWidth = Padding.Left + Padding.Right;
            var accumulatedHeight = Padding.Top + Padding.Bottom;

            foreach (var child in Children)
            {
                var childConstraint = new Size(
                    GetPositiveOrZero(availableSize.Width - accumulatedWidth),
                    GetPositiveOrZero(availableSize.Height - accumulatedHeight));

                child.Measure(childConstraint);
                var childDesiredSize = child.DesiredSize;

                switch ((Dock) child.GetValue(DockProperty))
                {
                    case Dock.Left:
                    case Dock.Right:
                        parentHeight = Math.Max(parentHeight, accumulatedHeight + childDesiredSize.Height);
                        accumulatedWidth += childDesiredSize.Width;
                        break;

                    case Dock.Top:
                    case Dock.Bottom:
                        parentWidth = Math.Max(parentWidth, accumulatedWidth + childDesiredSize.Width);
                        accumulatedHeight += childDesiredSize.Height;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            parentWidth = Math.Max(parentWidth, accumulatedWidth);
            parentHeight = Math.Max(parentHeight, accumulatedHeight);
            return new Size(parentWidth, parentHeight);
        }

        private static double GetPositiveOrZero(double value)
        {
            return Math.Max(value, 0);
        }
    }
}