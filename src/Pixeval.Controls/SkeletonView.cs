// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using CommunityToolkit.Labs.WinUI;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls;

namespace Pixeval.Controls;

public partial class SkeletonView : UniformGrid
{
    [GeneratedDependencyProperty]
    public partial ItemsViewLayoutType LayoutType { get; set; }

    [GeneratedDependencyProperty(DefaultValue = 250d)]
    public partial double MinItemHeight { get; set; }

    [GeneratedDependencyProperty(DefaultValue = 350d)]
    public partial double MinItemWidth { get; set; }

    public SkeletonView()
    {
        RowSpacing = 5;
        ColumnSpacing = 5;
        SizeChanged += (_, _) =>
        {
            Columns = double.IsInfinity(ActualWidth) || double.IsNaN(ActualWidth)
                ? 1
                : (int) Math.Ceiling(ActualWidth / MinItemWidth);
            Rows = double.IsInfinity(ActualHeight) || double.IsNaN(ActualHeight)
                ? 1
                : (int) Math.Ceiling(ActualHeight / MinItemHeight);
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
