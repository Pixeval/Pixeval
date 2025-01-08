// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;

namespace Pixeval.Pages.Capability;

public sealed partial class SpotlightsPage : IScrollViewHost
{
    public SpotlightsPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e)
    {
        ChangeSource();
    }

    private void ChangeSource()
    {
        SpotlightView.ViewModel.ResetEngine(App.AppViewModel.MakoClient.Spotlights());
    }

    public ScrollView ScrollView => SpotlightView.ScrollView;
}
