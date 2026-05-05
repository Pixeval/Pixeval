// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Misaki;
using IllustrationViewDataProvider = Pixeval.ViewModels.SharableViewDataProvider<
    Misaki.IArtworkInfo,
    Pixeval.ViewModels.IllustrationItemViewModel>;

namespace Pixeval.ViewModels;

public sealed class IllustrationViewViewModel : WorkViewViewModelBase<IArtworkInfo, IllustrationItemViewModel>
{
    public IllustrationViewViewModel(IllustrationViewViewModel viewModel) : this(viewModel.DataProvider.CloneRef(), viewModel.CachedBlockedTags)
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
        dataProvider.View.Filter = ((IWorkViewViewModel) this).DefaultFilter;
    }

    public override IllustrationViewDataProvider DataProvider { get; }

    public override bool RequireAdaptiveGrid => false;
}
