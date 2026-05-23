// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Mako.Model;
using SpotlightViewDataProvider = Pixeval.ViewModels.SimpleViewDataProvider<
    Mako.Model.Spotlight,
    Pixeval.ViewModels.SpotlightItemViewModel>;

namespace Pixeval.ViewModels;

public sealed class SpotlightViewViewModel : EntryViewViewModel<Spotlight, SpotlightItemViewModel>
{
    public override SpotlightViewDataProvider DataProvider { get; } = new();
}
