#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustratorViewDataProvider.cs
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
using Pixeval.Collections;
using Pixeval.Controls.Illustrate;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Misc;

namespace Pixeval.Controls.IllustratorView;

public class IllustratorViewDataProvider : DataProvider<User, IllustratorViewModel>
{
    public override AdvancedObservableCollection<IllustratorViewModel> View { get; } = [];

    private IncrementalLoadingCollection<FetchEngineIncrementalSource<User, IllustratorViewModel>, IllustratorViewModel> _illustratorsSource = null!;

    public override IncrementalLoadingCollection<FetchEngineIncrementalSource<User, IllustratorViewModel>, IllustratorViewModel> Source
    {
        get => _illustratorsSource;
        protected set
        {
            _ = SetProperty(ref _illustratorsSource, value);
            View.Source = value;
        }
    }

    public override IFetchEngine<User?>? FetchEngine { get; protected set; }

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

        Source = new(new IllustratorFetchEngineIncrementalSource(FetchEngine!, limit));
        var result = await Source.LoadMoreItemsAsync(20);
        return (int)result.Count;
    }
}
