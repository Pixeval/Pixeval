// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Controls;
using Pixeval.ViewModels;

namespace Pixeval.Views.Capability;

public partial class SpotlightsPage : ContentPage
{
    public SpotlightsPage()
    {
        InitializeComponent();
        ChangeSource();
    }

    private void ChangeSource()
    {
        (SpotlightView.DataContext as SpotlightViewViewModel)?.ResetEngine(App.AppViewModel.MakoClient.Spotlight(), (spotlight, _) => new(spotlight));
    }
}
