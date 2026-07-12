// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Mako.Model;
using SpotlightViewDataProvider = Pixeval.ViewModels.SharableViewDataProvider<
    Mako.Model.Spotlight,
    Pixeval.ViewModels.SpotlightItemViewModel>;

namespace Pixeval.ViewModels;

public sealed class SpotlightViewViewModel
    : EntryViewViewModel<Spotlight, SpotlightItemViewModel>, IRefCloneable<SpotlightViewViewModel>
{
    public SpotlightViewViewModel() : this(new SpotlightViewDataProvider())
    {
    }

    private SpotlightViewViewModel(SpotlightViewDataProvider dataProvider)
    {
        DataProvider = dataProvider;
    }

    public override SpotlightViewDataProvider DataProvider { get; }

    public SpotlightViewViewModel CloneRef() => new(DataProvider.CloneRef());
}
