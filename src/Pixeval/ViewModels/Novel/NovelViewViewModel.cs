// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using NovelViewDataProvider = Pixeval.ViewModels.SharableViewDataProvider<
    Mako.Model.Novel,
    Pixeval.ViewModels.NovelItemViewModel>;

namespace Pixeval.ViewModels;

public sealed class NovelViewViewModel : SortableEntryViewViewModel<Mako.Model.Novel, NovelItemViewModel>
{
    public NovelViewViewModel(NovelViewViewModel viewModel) : this(viewModel.DataProvider.CloneRef(), viewModel.BlockedTags)
    {
        Filter = viewModel.Filter;
        DataProvider.View.Range = viewModel.DataProvider.View.Range;
    }

    public NovelViewViewModel() : this(new NovelViewDataProvider(), null)
    {
    }

    private NovelViewViewModel(NovelViewDataProvider dataProvider, HashSet<string>? blockedTags) : base(blockedTags)
    {
        DataProvider = dataProvider;
        dataProvider.View.Filter = DefaultFilter;
    }

    public override NovelViewDataProvider DataProvider { get; }

    protected override void OnFilterChanged() => DataProvider.View.RaiseFilterChanged();
}
