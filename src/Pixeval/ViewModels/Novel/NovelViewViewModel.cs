// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Frozen;
using Mako.Model;
using NovelViewDataProvider = Pixeval.ViewModels.SharableViewDataProvider<
    Mako.Model.Novel,
    Pixeval.ViewModels.NovelItemViewModel>;

namespace Pixeval.ViewModels;

public sealed class NovelViewViewModel
    : WorkViewViewModelBase<Novel, NovelItemViewModel>, IRefCloneable<NovelViewViewModel>
{
    public NovelViewViewModel() : this(new NovelViewDataProvider(), null)
    {
    }

    private NovelViewViewModel(NovelViewDataProvider dataProvider, FrozenSet<string>? blockedTags) : base(blockedTags)
    {
        DataProvider = dataProvider;
        SetFilters();
    }

    public override NovelViewDataProvider DataProvider { get; }

    public override bool RequireAdaptiveGrid => true;

    public NovelViewViewModel CloneRef() => new(DataProvider.CloneRef(), CachedBlockedTags);
}
