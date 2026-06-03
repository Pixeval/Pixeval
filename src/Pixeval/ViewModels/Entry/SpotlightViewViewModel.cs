// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Mako.Model;
using SpotlightViewDataProvider = Pixeval.ViewModels.SharableViewDataProvider<
    Mako.Model.Spotlight,
    Pixeval.ViewModels.SpotlightItemViewModel>;

namespace Pixeval.ViewModels;

public sealed class SpotlightViewViewModel : EntryViewViewModel<Spotlight, SpotlightItemViewModel>
{
    public SpotlightViewViewModel(SpotlightViewViewModel viewModel) : this(viewModel.DataProvider.CloneRef())
    {
    }

    public SpotlightViewViewModel() : this(new SpotlightViewDataProvider())
    {
    }

    private SpotlightViewViewModel(SpotlightViewDataProvider dataProvider)
    {
        DataProvider = dataProvider;
    }

    public override SpotlightViewDataProvider DataProvider { get; }
}
