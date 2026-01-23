using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Pixeval.ViewModels;

namespace Pixeval;

public partial class SpotlightsPage : UserControl
{
    public SpotlightsPage()
    {
        InitializeComponent();
        AddHandler(Frame.NavigatedToEvent, (sender, e) => ChangeSource());
    }

    private void ChangeSource()
    {
        (SpotlightView.DataContext as SpotlightViewViewModel)?.ResetEngine(App.AppViewModel.MakoClient.Spotlights());
    }
}
