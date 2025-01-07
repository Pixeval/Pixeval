// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
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
    private static void OnLayoutTypeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
    }

    public SkeletonView()
    {
        RowSpacing = 5;
        ColumnSpacing = 5;
        SizeChanged += (_, _) =>
        {
            Columns = ActualWidth is double.PositiveInfinity or double.NegativeInfinity or double.NaN
                ? 1 : (int)Math.Ceiling(ActualWidth / MinItemWidth);
            Rows = ActualHeight is double.PositiveInfinity or double.NegativeInfinity or double.NaN
                ? 1 : (int)Math.Ceiling(ActualHeight / MinItemHeight);
            var count = Rows * Columns;
            while (count != Children.Count)
            {
                if (count > Children.Count)
                    Children.Add(new Shimmer());
                else
                    Children.RemoveAt(Children.Count - 1);
            }
        };
    }
}
