// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml.Navigation;
using Mako.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Pages.Capability;

public sealed partial class RecommendationPage
{
    public RecommendationPage()
    {
        InitializeComponent();
    }

    public override void OnPageActivated(NavigationEventArgs e, object? parameter) => ChangeSource();

    private void WorkTypeComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) => ChangeSource();

    private async void ChangeSource()
    {
        WorkContainer.WorkView.ResetEngine(await App.AppViewModel.GetEngineAsync<Illustration>("recommendation/illustrations"));
    }

    private void WorkContainer_OnRefreshRequested(object sender, RoutedEventArgs e) => ChangeSource();
}
