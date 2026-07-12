// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Mako.Engine;
using Mako.Model;
using Pixeval.ViewModels;

namespace Pixeval.Views.Capability;

public partial class SpotlightPage : IconContentPage
{
    public SpotlightPage() : this(null)
    {
    }

    public SpotlightPage(SpotlightViewViewModel? viewModel)
    {
        InitializeComponent();
        if (viewModel is not null)
            SpotlightView.SetViewModel(viewModel);
        else
        {
            ChangeSource();
        }
    }

    private void ChangeSource()
    {
        ResetEngine(App.AppViewModel.MakoClient.Spotlight());
    }

    private void ResetEngine(IFetchEngine<Spotlight> fetchEngine) =>
        (SpotlightView.DataContext as SpotlightViewViewModel)?.ResetEngine(fetchEngine, static (spotlight, _) => new(spotlight));
}
