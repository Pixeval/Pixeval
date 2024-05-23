#region Copyright
// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2024 Pixeval.Controls/SkeletonView.cs
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
using CommunityToolkit.Labs.WinUI;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<ItemsViewLayoutType>("LayoutType", DependencyPropertyDefaultValue.Default, nameof(OnLayoutTypeChanged))]
[DependencyProperty<double>("MinItemHeight", "250d")]
[DependencyProperty<double>("MinItemWidth", "350d")]
public partial class SkeletonView : UniformGrid
{
    protected override Size MeasureOverride(Size availableSize)
    {
        Columns = availableSize.Width is double.PositiveInfinity or double.NegativeInfinity or double.NaN
            ? 1 : (int)Math.Ceiling(availableSize.Width / MinItemWidth);
        Rows = availableSize.Height is double.PositiveInfinity or double.NegativeInfinity or double.NaN
            ? 1 : (int)Math.Ceiling(availableSize.Height / MinItemHeight);
        var count = Rows * Columns;
        while (count != Children.Count)
        {
            if (count > Children.Count)
                Children.Add(new Shimmer());
            else
                Children.RemoveAt(Children.Count - 1);
        }
        return base.MeasureOverride(availableSize);
    }

    private static void OnLayoutTypeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
    }

    public SkeletonView()
    {
        RowSpacing = 5;
        ColumnSpacing = 5;
    }
}
