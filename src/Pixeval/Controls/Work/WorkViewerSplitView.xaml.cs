// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Controls.Windowing;

namespace Pixeval.Controls;

public sealed partial class WorkViewerSplitView
{
    [GeneratedDependencyProperty]
    public partial object? MenuItemsSource { get; set; }

    [GeneratedDependencyProperty]
    public partial object? PaneContent { get; set; }

    [GeneratedDependencyProperty(DefaultValue = false)]
    public partial bool IsPaneOpen { get; set; }

    public const double OpenPaneLength = 330;

    public WorkViewerSplitView() => InitializeComponent();

    private void NavigationViewOnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (sender.SelectedItem is NavigationViewTag tag)
            _ = PaneFrame.Navigate(tag.NavigateTo, tag.Parameter, e.RecommendedNavigationTransitionInfo);
    }

    public void NavigationViewSelectRefresh(NavigationTransitionInfo? info = null)
    {
        if (NavigationView.SelectedItem is NavigationViewTag tag)
            _ = PaneFrame.Navigate(tag.NavigateTo, tag.Parameter, info);
    }
}
