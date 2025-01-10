// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls.Windowing;

[DependencyProperty<NavigationViewTag>("SelectedItem", IsNullable = true)]
public sealed partial class TabPage
{
    public ObservableCollection<NavigationViewTag> ViewModels { get; } = [];

    public event TypedEventHandler<TabPage, TabClosedEventArgs>? TabClosed;

    public TabPage()
    {
        InitializeComponent();
    }

    public void AddPage<T>(NavigationViewTag<T> viewModel) where T : EnhancedWindowPage
    {
        ViewModels.Add(viewModel);
        SelectedItem = viewModel;
    }

    public void AddPage(NavigationViewTag viewModel)
    {
        ViewModels.Add(viewModel);
        SelectedItem = viewModel;
    }

    private void Frame_OnLoaded(object sender, RoutedEventArgs e)
    {
        var frame = sender.To<Frame>();
        if (frame.Content is not null)
            return;
        var viewModel = frame.GetTag<NavigationViewTag>();
        _ = frame.Navigate(viewModel.NavigateTo, viewModel.Parameter, viewModel.TransitionInfo);
    }

    private void TabView_OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs e)
    {
        if (e.Item is not NavigationViewTag tag || e.Tab.Content is not Frame { Content: Page page })
            return;

        _ = ViewModels.Remove(tag);
        TabClosed?.Invoke(this, new TabClosedEventArgs(tag, page));
    }
}

public class TabClosedEventArgs(NavigationViewTag tag, Page page) : EventArgs
{
    public NavigationViewTag Tag { get; } = tag;

    public Page Page { get; } = page;
}
