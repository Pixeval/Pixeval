// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Pixeval.CoreApi.Model;
using IllustrationViewDataProvider = Pixeval.Controls.SharableViewDataProvider<
    Pixeval.CoreApi.Model.Illustration,
    Pixeval.Controls.IllustrationItemViewModel>;

namespace Pixeval.Controls;

public sealed partial class IllustrationViewViewModel : SortableEntryViewViewModel<Illustration, IllustrationItemViewModel>
{
    public IllustrationViewViewModel(IllustrationViewViewModel viewModel) : this(viewModel.DataProvider.CloneRef(), viewModel.BlockedTags)
    {
        Filter = viewModel.Filter;
        DataProvider.View.Range = viewModel.DataProvider.View.Range;
    }

    public IllustrationViewViewModel() : this(new IllustrationViewDataProvider(), App.AppViewModel.AppSettings.BlockedTags)
    {

    }

    private IllustrationViewViewModel(IllustrationViewDataProvider dataProvider, HashSet<string> blockedTags) : base(blockedTags)
    {
        DataProvider = dataProvider;
        dataProvider.View.Filter = DefaultFilter;
        dataProvider.View.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoItem));
    }

    public override IllustrationViewDataProvider DataProvider { get; }

    protected override void OnFilterChanged() => DataProvider.View.RaiseFilterChanged();
}
