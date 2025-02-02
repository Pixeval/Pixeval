// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using CommunityToolkit.WinUI;

namespace Pixeval.Controls;

public partial class AdvancedItemsView
{
    [GeneratedDependencyProperty(DefaultValue = ItemsViewLayoutType.LinedFlow)]
    public partial ItemsViewLayoutType LayoutType { get; set; }

    [GeneratedDependencyProperty(DefaultValue = 0d)]
    public partial double MinItemHeight { get; set; }

    [GeneratedDependencyProperty(DefaultValue = 0d)]
    public partial double MinItemWidth { get; set; }

    [GeneratedDependencyProperty(DefaultValue = 5d)]
    public partial double MinRowSpacing { get; set; }

    [GeneratedDependencyProperty(DefaultValue = 5d)]
    public partial double MinColumnSpacing { get; set; }

    [GeneratedDependencyProperty(DefaultValue = 100d)]
    public partial double LoadingOffset { get; set; }

    [GeneratedDependencyProperty(DefaultValue = -1)]
    public partial int SelectedIndex { get; set; }

    [GeneratedDependencyProperty(DefaultValue = true)]
    public partial bool CanLoadMore { get; set; }

    [GeneratedDependencyProperty(DefaultValue = false)]
    public partial bool IsLoadingMore { get; set; }

    [GeneratedDependencyProperty(DefaultValue = 20)]
    public partial int LoadCount { get; set; }
}
