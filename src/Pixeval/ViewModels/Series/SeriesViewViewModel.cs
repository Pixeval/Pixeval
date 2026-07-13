// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using SeriesViewDataProvider = Pixeval.ViewModels.SharableViewDataProvider<
    Mako.Model.Series,
    Pixeval.ViewModels.SeriesItemViewModel>;

namespace Pixeval.ViewModels;

public sealed class SeriesViewViewModel
    : EntryViewViewModel<Series, SeriesItemViewModel>, IRefCloneable<SeriesViewViewModel>
{
    public SeriesViewViewModel() : this(new SeriesViewDataProvider())
    {
    }

    private SeriesViewViewModel(SeriesViewDataProvider dataProvider)
    {
        DataProvider = dataProvider;
    }

    public override SeriesViewDataProvider DataProvider { get; }

    public void ResetEngine(IFetchEngine<Series> fetchEngine, SimpleWorkType workType) =>
        base.ResetEngine(fetchEngine, (series, _) => new(series, workType));

    public SeriesViewViewModel CloneRef() => new(DataProvider.CloneRef());
}
