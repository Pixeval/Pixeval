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
        (SpotlightView.DataContext as SpotlightViewViewModel)?.ResetEngine(App.AppViewModel.MakoClient.Spotlights(), (spotlight, _) => new(spotlight));
    }
}
