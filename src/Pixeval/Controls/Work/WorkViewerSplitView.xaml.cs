// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Controls.Windowing;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<object>("MenuItemsSource")]
[DependencyProperty<object>("PaneContent")]
[DependencyProperty<bool>("IsPaneOpen", "false")]
public sealed partial class WorkViewerSplitView
{
    public const double OpenPaneLength = 330;

    public WorkViewerSplitView() => InitializeComponent();

    private void NavigationViewOnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (sender.SelectedItem is NavigationViewTag tag)
            NavigationViewSelect(tag, e.RecommendedNavigationTransitionInfo);
    }

    public void NavigationViewSelect(NavigationViewTag tag, NavigationTransitionInfo? info = null)
    {
        if (NavigationView.SelectedItem.To<NavigationViewTag>() == tag)
            _ = PaneFrame.Navigate(tag.NavigateTo, tag.Parameter, info);
    }
}
