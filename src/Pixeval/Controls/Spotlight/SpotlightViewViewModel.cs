// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.CoreApi.Model;
using SpotlightViewDataProvider = Pixeval.Controls.SimpleViewDataProvider<
    Pixeval.CoreApi.Model.Spotlight,
    Pixeval.Controls.SpotlightItemViewModel>;

namespace Pixeval.Controls;

public sealed partial class SpotlightViewViewModel : EntryViewViewModel<Spotlight, SpotlightItemViewModel>
{
    public override SpotlightViewDataProvider DataProvider { get; } = new();

    public SpotlightViewViewModel() => DataProvider.View.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoItem));
}
