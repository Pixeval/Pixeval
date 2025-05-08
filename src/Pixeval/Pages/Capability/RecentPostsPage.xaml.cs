// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Mako.Global.Enum;
using Mako.Model;
using Microsoft.UI.Xaml;

namespace Pixeval.Pages.Capability;

public sealed partial class RecentPostsPage : IScrollViewHost
{
    public RecentPostsPage()
    {
        InitializeComponent();
    }

    public override void OnPageActivated(NavigationEventArgs e, object? parameter) => ChangeSource();

    private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => ChangeSource();

    private async void ChangeSource()
    {
        WorkContainer.WorkView.ResetEngine(await App.AppViewModel.GetEngineAsync<Illustration>("newworks"));
    }

    public ScrollView ScrollView => WorkContainer.ScrollView;

    private void WorkContainer_OnRefreshRequested(object sender, RoutedEventArgs e) => ChangeSource();
}
