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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Pixeval.Collections;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;

namespace Pixeval.Controls;

public class IllustratorViewDataProvider : ObservableObject, IDataProvider<User, IllustratorItemViewModel>
{
    private IFetchEngine<User?>? _fetchEngine;

    public AdvancedObservableCollection<IllustratorItemViewModel> View { get; } = [];

    public IncrementalLoadingCollection<FetchEngineIncrementalSource<User, IllustratorItemViewModel>, IllustratorItemViewModel> Source
    {
        get => (View.Source as IncrementalLoadingCollection<FetchEngineIncrementalSource<User, IllustratorItemViewModel>, IllustratorItemViewModel>)!;
        protected set => View.Source = value;
    }

    public IFetchEngine<User?>? FetchEngine
    {
        get => _fetchEngine;
        protected set
        {
            if (value == _fetchEngine)
                return;
            _fetchEngine?.EngineHandle.Cancel();
            _fetchEngine = value;
        }
    }

    public void Dispose()
    {
        if (Source is { } source)
            foreach (var illustratorViewModel in source)
                illustratorViewModel.Dispose();

        FetchEngine = null;
    }

    public void ResetEngine(IFetchEngine<User?>? fetchEngine, int limit = -1)
    {
        Dispose();
        FetchEngine = fetchEngine;

        Source = new IncrementalLoadingCollection<FetchEngineIncrementalSource<User, IllustratorItemViewModel>, IllustratorItemViewModel>(new IllustratorFetchEngineIncrementalSource(FetchEngine!, limit));
    }
}
