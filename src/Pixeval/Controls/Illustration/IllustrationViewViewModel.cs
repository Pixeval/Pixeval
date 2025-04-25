// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Misaki;
using IllustrationViewDataProvider = Pixeval.Controls.SharableViewDataProvider<
    Misaki.IArtworkInfo,
    Pixeval.Controls.IllustrationItemViewModel>;

namespace Pixeval.Controls;

public sealed partial class IllustrationViewViewModel : SortableEntryViewViewModel<IArtworkInfo, IllustrationItemViewModel>
{
    public IllustrationViewViewModel(IllustrationViewViewModel viewModel) : this(viewModel.DataProvider.CloneRef(), viewModel.BlockedTags)
    {
        Filter = viewModel.Filter;
        DataProvider.View.Range = viewModel.DataProvider.View.Range;
    }

    public IllustrationViewViewModel() : this(new IllustrationViewDataProvider(), null)
    {
    }

    private IllustrationViewViewModel(IllustrationViewDataProvider dataProvider, HashSet<string>? blockedTags) : base(blockedTags)
    {
        DataProvider = dataProvider;
        dataProvider.View.Filter = DefaultFilter;
        dataProvider.View.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoItem));
    }

    public override IllustrationViewDataProvider DataProvider { get; }

    protected override void OnFilterChanged() => DataProvider.View.RaiseFilterChanged();
}
