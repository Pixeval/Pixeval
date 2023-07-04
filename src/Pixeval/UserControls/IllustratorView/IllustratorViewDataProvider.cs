#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/GridIllustratorViewDataProvider.cs
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
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.UI;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Misc;

namespace Pixeval.UserControls.IllustratorView;

public class IllustratorViewDataProvider : ObservableObject, IIllustratorViewDataProvider
{
    public IllustratorViewDataProvider()
    {
        _illustratorsSource = new ObservableCollection<IllustratorViewModel>();
        IllustratorsView = new AdvancedCollectionView(IllustratorsSource);
    }

    public AdvancedCollectionView IllustratorsView { get; }

    private ObservableCollection<IllustratorViewModel> _illustratorsSource;

    public ObservableCollection<IllustratorViewModel> IllustratorsSource
    {
        get => _illustratorsSource;
        set
        {
            SetProperty(ref _illustratorsSource, value);
            IllustratorsView.Source = value;
        }
    }

    public IFetchEngine<User?>? FetchEngine { get; private set; }

    private Predicate<object>? _filter;

    public Predicate<object>? Filter
    {
        get => _filter;
        set
        {
            _filter = value;
            _filterChanged?.Invoke(_filter, EventArgs.Empty);
        }
    }

    private EventHandler? _filterChanged;

    public event EventHandler? FilterChanged
    {
        add => _filterChanged += value;
        remove => _filterChanged -= value;
    }

    public void DisposeCurrent()
    {
        foreach (var illustratorViewModel in IllustratorsSource)
        {
            illustratorViewModel.Dispose();
        }

        IllustratorsView.Clear();
    }

    public async Task<int> LoadMore()
    {
        if (IllustratorsSource is IncrementalLoadingCollection<FetchEngineIncrementalSource<User, IllustratorViewModel>, IllustratorViewModel> coll)
        {
            return (int)(await coll.LoadMoreItemsAsync(20)).Count;
        }

        return 0;
    }

    public async Task<int> FillAsync(int? itemsLimit = null)
    {
        var collection = new IncrementalLoadingCollection<FetchEngineIncrementalSource<User, IllustratorViewModel>, IllustratorViewModel>(new IllustratorFetchEngineIncrementalSource(FetchEngine!, itemsLimit));
        IllustratorsSource = collection;
        var result = await collection.LoadMoreItemsAsync(20);
        return (int)result.Count;
    }

    public Task<int> ResetAndFillAsync(IFetchEngine<User?>? fetchEngine, int? itemLimit = null)
    {
        FetchEngine?.EngineHandle.Cancel();
        FetchEngine = fetchEngine;
        DisposeCurrent();
        return FillAsync(itemLimit);
    }
}