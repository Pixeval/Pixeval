// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.Controls;

namespace Pixeval.Pages.Capability.Feeds;

public partial class FeedPageViewModel : EntryViewViewModel<IFeedEntry, AbstractFeedItemViewModel>
{
    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    private CancellationTokenSource _loadingCancellation = new();

    public FeedPageViewModel(SharableViewDataProvider<IFeedEntry, AbstractFeedItemViewModel> dataProvider)
    {
        DataProvider = dataProvider;
        dataProvider.View.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoItem));
    }

    public FeedPageViewModel() : this(new SharableViewDataProvider<IFeedEntry, AbstractFeedItemViewModel>())
    {
    }

    public override IDataProvider<IFeedEntry, AbstractFeedItemViewModel> DataProvider { get; }

    public void CancelLoad()
    {
        _loadingCancellation.Cancel(false);
        _loadingCancellation = new CancellationTokenSource();
    }

    public async Task<T> PerformLoadAsync<T>(Func<Task<T>> func)
    {
        IsLoading = true;
        var result = await func();

        if (_loadingCancellation.IsCancellationRequested)
            return await Task.FromCanceled<T>(_loadingCancellation.Token);

        IsLoading = false;
        return result;
    }
}
