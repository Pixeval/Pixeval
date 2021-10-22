#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/WrapPanel.Data.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation;
using Microsoft.UI.Xaml.Controls;

namespace Pixeval.CommunityToolkit.WrapPanel
{
    /// <summary>
    ///     WrapPanel is a panel that position child control vertically or horizontally based on the orientation and when max
    ///     width/ max height is received a new row(in case of horizontal) or column (in case of vertical) is created to fit
    ///     new controls.
    /// </summary>
    public partial class WrapPanel
    {
        [DebuggerDisplay("U = {U} V = {V}")]
        private struct UvMeasure
        {
            internal static UvMeasure Zero => default;

            internal double U { get; set; }

            internal double V { get; set; }

            public UvMeasure(Orientation orientation, Size size)
                : this(orientation, size.Width, size.Height)
            {
            }

            public UvMeasure(Orientation orientation, double width, double height)
            {
                if (orientation == Orientation.Horizontal)
                {
                    U = width;
                    V = height;
                }
                else
                {
                    U = height;
                    V = width;
                }
            }

            public UvMeasure Add(double u, double v)
            {
                return new UvMeasure { U = U + u, V = V + v };
            }

            public UvMeasure Add(UvMeasure measure)
            {
                return Add(measure.U, measure.V);
            }

            public Size ToSize(Orientation orientation)
            {
                return orientation == Orientation.Horizontal ? new Size(U, V) : new Size(V, U);
            }
        }

        private readonly struct UvRect
        {
            public UvMeasure Position { get; init; }

            public UvMeasure Size { get; init; }

            public Rect ToRect(Orientation orientation)
            {
                return orientation switch
                {
                    Orientation.Vertical => new Rect(Position.V, Position.U, Size.V, Size.U),
                    Orientation.Horizontal => new Rect(Position.U, Position.V, Size.U, Size.V),
                    _ => ThrowArgumentException()
                };
            }

            private static Rect ThrowArgumentException()
            {
                throw new ArgumentException("The input orientation is not valid.");
            }
        }

        private struct Row
        {
            public Row(List<UvRect> childrenRects, UvMeasure size)
            {
                ChildrenRects = childrenRects;
                Size = size;
            }

            public List<UvRect> ChildrenRects { get; }

            public UvMeasure Size { get; private set; }

            public UvRect Rect => ChildrenRects.Count > 0 ? new UvRect { Position = ChildrenRects[0].Position, Size = Size } : new UvRect { Position = UvMeasure.Zero, Size = Size };

            public void Add(UvMeasure position, UvMeasure size)
            {
                ChildrenRects.Add(new UvRect { Position = position, Size = size });
                Size = new UvMeasure
                {
                    U = position.U + size.U,
                    V = Math.Max(Size.V, size.V)
                };
            }
        }
    }
}