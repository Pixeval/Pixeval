#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/FeedPageViewModel.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.Controls;
using Pixeval.CoreApi.Model;

namespace Pixeval.Pages.Capability.Feeds;

public partial class FeedPageViewModel : EntryViewViewModel<IFeedEntry, AbstractFeedItemViewModel>
{
    [ObservableProperty]
    private bool _isLoading;

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
