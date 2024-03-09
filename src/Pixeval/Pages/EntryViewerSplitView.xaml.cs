using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Controls.Windowing;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Pages;

[DependencyProperty<object>("MenuItemsSource")]
[DependencyProperty<object>("PaneContent")]
[DependencyProperty<bool>("IsPaneOpen", "false", nameof(OnIsPaneOpenChanged))]
[DependencyProperty<bool>("PinPane", "false", nameof(OnPinPaneChanged))]
public sealed partial class EntryViewerSplitView
{
    public event EventHandler? RaiseSetTitleBarDragRegion;

    public const double OpenPaneLength = 330;

    public EntryViewerSplitView() => InitializeComponent();

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

    private void SplitView_OnPaneOpenedOrClosed(SplitView sender, object e) => RaiseSetTitleBarDragRegion?.Invoke(this, EventArgs.Empty);

    private static void OnIsPaneOpenChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        if (o is EntryViewerSplitView { IsPaneOpen: false, PinPane: true } splitView)
            splitView.PinPane = false;
    }

    private static void OnPinPaneChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        if (o is EntryViewerSplitView splitView)
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
