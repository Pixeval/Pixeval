using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Controls.Windowing;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<object>("MenuItemsSource")]
[DependencyProperty<object>("PaneContent")]
[DependencyProperty<bool>("IsPaneOpen", "false", nameof(OnIsPaneOpenChanged))]
[DependencyProperty<bool>("PinPane", "false", nameof(OnPinPaneChanged))]
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

    private static void OnIsPaneOpenChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        if (o is WorkViewerSplitView { IsPaneOpen: false, PinPane: true } splitView)
            splitView.PinPane = false;
    }

    private static void OnPinPaneChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        if (o is WorkViewerSplitView splitView)
            if (splitView.PinPane)
            {
                splitView.SplitView.DisplayMode = SplitViewDisplayMode.Inline;
                splitView.IsPaneOpen = true;
            }
            else
                splitView.SplitView.DisplayMode = SplitViewDisplayMode.Overlay;
    }

    private void NavigationView_OnLoaded(object sender, RoutedEventArgs e)
    {
        var navigationView = sender.To<NavigationView>();
        navigationView.SelectedItem = (navigationView.MenuItemsSource as IEnumerable<NavigationViewTag>)?.FirstOrDefault();
    }
}
