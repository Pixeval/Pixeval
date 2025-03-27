// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Mako.Model;
using NovelViewDataProvider = Pixeval.Controls.SharableViewDataProvider<
    Mako.Model.Novel,
    Pixeval.Controls.NovelItemViewModel>;

namespace Pixeval.Controls;

public sealed partial class NovelViewViewModel : SortableEntryViewViewModel<Novel, NovelItemViewModel>
{
    public NovelViewViewModel(NovelViewViewModel viewModel) : this(viewModel.DataProvider.CloneRef(), viewModel.BlockedTags)
    {
        Filter = viewModel.Filter;
        DataProvider.View.Range = viewModel.DataProvider.View.Range;
    }

    public NovelViewViewModel() : this(new NovelViewDataProvider(), App.AppViewModel.AppSettings.BlockedTags)
    {
    }

    private NovelViewViewModel(NovelViewDataProvider dataProvider, HashSet<string> blockedTags) : base(blockedTags)
    {
        DataProvider = dataProvider;
        dataProvider.View.Filter = DefaultFilter;
        dataProvider.View.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoItem));
    }

    public override NovelViewDataProvider DataProvider { get; }

    protected override void OnFilterChanged() => DataProvider.View.RaiseFilterChanged();
}
