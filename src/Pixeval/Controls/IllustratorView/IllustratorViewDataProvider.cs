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
using CommunityToolkit.WinUI.Collections;
using Pixeval.Controls.Illustrate;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Misc;

namespace Pixeval.Controls.IllustratorView;

public class IllustratorViewDataProvider : DataProvider<User, IllustratorProfileViewModel>
{
    public override AdvancedCollectionView View { get; } = new(Array.Empty<IllustratorProfileViewModel>());

    private IncrementalLoadingCollection<FetchEngineIncrementalSource<User, IllustratorProfileViewModel>, IllustratorProfileViewModel> _illustratorsSource = null!;

    public override IncrementalLoadingCollection<FetchEngineIncrementalSource<User, IllustratorProfileViewModel>, IllustratorProfileViewModel> Source
    {
        get => _illustratorsSource;
        protected set
        {
            _ = SetProperty(ref _illustratorsSource, value);
            View.Source = value;
        }
    }

    public override IFetchEngine<User?>? FetchEngine { get; protected set; }

    private Predicate<object>? _filter;

    public override Predicate<object>? Filter
    {
        get => _filter;
        set
        {
            _filter = value;
            FilterChanged?.Invoke(_filter, EventArgs.Empty);
        }
    }

    public override event EventHandler? FilterChanged;

    public override void DisposeCurrent()
    {
        if (Source is { } source)
            foreach (var illustratorViewModel in source)
            {
                illustratorViewModel.Dispose();
            }

        View.Clear();
    }

    public override async Task<int> ResetAndFillAsync(IFetchEngine<User?>? fetchEngine, int limit = -1)
    {
        FetchEngine?.EngineHandle.Cancel();
        FetchEngine = fetchEngine;
        DisposeCurrent();

        var collection = new IncrementalLoadingCollection<FetchEngineIncrementalSource<User, IllustratorProfileViewModel>, IllustratorProfileViewModel>(new IllustratorFetchEngineIncrementalSource(FetchEngine!, limit));
        Source = collection;
        var result = await collection.LoadMoreItemsAsync(20);
        return (int)result.Count;
    }
}
