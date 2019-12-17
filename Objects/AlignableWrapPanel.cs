// Pixeval
// Copyright (C) 2019 Dylech30th <decem0730@gmail.com>
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
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows;
using System.Windows.Controls;

namespace Pixeval.Objects
{
    public class AlignableWrapPanel : Panel
    {
        public static readonly DependencyProperty HorizontalContentAlignmentProperty =
            DependencyProperty.Register("HorizontalContentAlignment", typeof(HorizontalAlignment), typeof(AlignableWrapPanel), new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.AffectsArrange));

        public HorizontalAlignment HorizontalContentAlignment
        {
            get => (HorizontalAlignment) GetValue(HorizontalContentAlignmentProperty);
            set => SetValue(HorizontalContentAlignmentProperty, value);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var curLineSize = new Size();
            var panelSize = new Size();

            var children = InternalChildren;

            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i];

                child.Measure(constraint);
                var sz = child.DesiredSize;

                if (curLineSize.Width + sz.Width > constraint.Width)
                {
                    panelSize.Width = Math.Max(curLineSize.Width, panelSize.Width);
                    panelSize.Height += curLineSize.Height;
                    curLineSize = sz;

                    if (sz.Width > constraint.Width)
                    {
                        panelSize.Width = Math.Max(sz.Width, panelSize.Width);
                        panelSize.Height += sz.Height;
                        curLineSize = new Size();
                    }
                }
                else
                {
                    curLineSize.Width += sz.Width;
                    curLineSize.Height = Math.Max(sz.Height, curLineSize.Height);
                }
            }

            panelSize.Width = Math.Max(curLineSize.Width, panelSize.Width);
            panelSize.Height += curLineSize.Height;

            return panelSize;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var firstInLine = 0;
            var curLineSize = new Size();
            double accumulatedHeight = 0;
            var children = InternalChildren;

            for (var i = 0; i < children.Count; i++)
            {
                var sz = children[i].DesiredSize;

                if (curLineSize.Width + sz.Width > arrangeBounds.Width)
                {
                    ArrangeLine(accumulatedHeight, curLineSize, arrangeBounds.Width, firstInLine, i);

                    accumulatedHeight += curLineSize.Height;
                    curLineSize = sz;

                    if (sz.Width > arrangeBounds.Width)
                    {
                        ArrangeLine(accumulatedHeight, sz, arrangeBounds.Width, i, ++i);
                        accumulatedHeight += sz.Height;
                        curLineSize = new Size();
                    }

                    firstInLine = i;
                }
                else
                {
                    curLineSize.Width += sz.Width;
                    curLineSize.Height = Math.Max(sz.Height, curLineSize.Height);
                }
            }

            if (firstInLine < children.Count)
                ArrangeLine(accumulatedHeight, curLineSize, arrangeBounds.Width, firstInLine, children.Count);

            return arrangeBounds;
        }

        private void ArrangeLine(double y, Size lineSize, double boundsWidth, int start, int end)
        {
            var x = HorizontalContentAlignment switch
            {
                HorizontalAlignment.Center => ((boundsWidth - lineSize.Width) / 2),
                HorizontalAlignment.Right  => (boundsWidth - lineSize.Width),
                _                          => 0
            };

            var children = InternalChildren;
            for (var i = start; i < end; i++)
            {
                var child = children[i];
                child.Arrange(new Rect(x, y, child.DesiredSize.Width, lineSize.Height));
                x += child.DesiredSize.Width;
            }
        }
    }
}