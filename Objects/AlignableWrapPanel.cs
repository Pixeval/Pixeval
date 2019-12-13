using System;
using System.Windows;
using System.Windows.Controls;

namespace Pixeval.Objects
{
    public class AlignableWrapPanel : Panel
    {
        public HorizontalAlignment HorizontalContentAlignment
        {
            get => (HorizontalAlignment)GetValue(HorizontalContentAlignmentProperty);
            set => SetValue(HorizontalContentAlignmentProperty, value);
        }

        public static readonly DependencyProperty HorizontalContentAlignmentProperty =
            DependencyProperty.Register("HorizontalContentAlignment", typeof(HorizontalAlignment), typeof(AlignableWrapPanel), new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.AffectsArrange));

        protected override Size MeasureOverride(Size constraint)
        {
            var curLineSize = new Size();
            var panelSize = new Size();

            var children = InternalChildren;

            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i] as UIElement;

                // Flow passes its own constraint to children
                child.Measure(constraint);
                var sz = child.DesiredSize;

                if (curLineSize.Width + sz.Width > constraint.Width) //need to switch to another line
                {
                    panelSize.Width = Math.Max(curLineSize.Width, panelSize.Width);
                    panelSize.Height += curLineSize.Height;
                    curLineSize = sz;

                    if (sz.Width > constraint.Width) // if the element is wider then the constraint - give it a separate line                    
                    {
                        panelSize.Width = Math.Max(sz.Width, panelSize.Width);
                        panelSize.Height += sz.Height;
                        curLineSize = new Size();
                    }
                }
                else //continue to accumulate a line
                {
                    curLineSize.Width += sz.Width;
                    curLineSize.Height = Math.Max(sz.Height, curLineSize.Height);
                }
            }

            // the last line size, if any need to be added
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

                if (curLineSize.Width + sz.Width > arrangeBounds.Width) //need to switch to another line
                {
                    ArrangeLine(accumulatedHeight, curLineSize, arrangeBounds.Width, firstInLine, i);

                    accumulatedHeight += curLineSize.Height;
                    curLineSize = sz;

                    if (sz.Width > arrangeBounds.Width) //the element is wider then the constraint - give it a separate line                    
                    {
                        ArrangeLine(accumulatedHeight, sz, arrangeBounds.Width, i, ++i);
                        accumulatedHeight += sz.Height;
                        curLineSize = new Size();
                    }
                    firstInLine = i;
                }
                else //continue to accumulate a line
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