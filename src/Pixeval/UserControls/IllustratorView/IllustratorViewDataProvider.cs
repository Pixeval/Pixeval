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
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.UI;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Misc;
using Pixeval.UserControls.Illustrate;

namespace Pixeval.UserControls.IllustratorView;

public class IllustratorViewDataProvider : ObservableObject, IDataProvider<User, IllustratorViewModel>
{
    public AdvancedCollectionView View { get; } = new(Array.Empty<IllustratorViewModel>());

    private IncrementalLoadingCollection<FetchEngineIncrementalSource<User, IllustratorViewModel>, IllustratorViewModel> _illustratorsSource = null!;

    public IncrementalLoadingCollection<FetchEngineIncrementalSource<User, IllustratorViewModel>, IllustratorViewModel> Source
    {
        get => _illustratorsSource;
        set
        {
            SetProperty(ref _illustratorsSource, value);
            View.Source = value;
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
            FilterChanged?.Invoke(_filter, EventArgs.Empty);
        }
    }

    public event EventHandler? FilterChanged;

    public void DisposeCurrent()
    {
        if (Source is { } source)
            foreach (var illustratorViewModel in source)
            {
                illustratorViewModel.Dispose();
            }

        View.Clear();
    }

    public async Task<int> ResetAndFillAsync(IFetchEngine<User?>? fetchEngine, int limit = -1)
    {
        FetchEngine?.EngineHandle.Cancel();
        FetchEngine = fetchEngine;
        DisposeCurrent();

        var collection = new IncrementalLoadingCollection<FetchEngineIncrementalSource<User, IllustratorViewModel>, IllustratorViewModel>(new IllustratorFetchEngineIncrementalSource(FetchEngine!, limit));
        Source = collection;
        var result = await collection.LoadMoreItemsAsync(20);
        return (int)result.Count;
    }
}
