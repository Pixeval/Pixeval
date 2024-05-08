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
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Pixeval.Collections;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;

namespace Pixeval.Controls;

public class SimpleViewDataProvider<T, TViewModel> : ObservableObject, IDataProvider<T, TViewModel> 
    where T : class, IIdEntry
    where TViewModel : class, IViewModelFactory<T, TViewModel>, IDisposable
{
    private IFetchEngine<T>? _fetchEngine;

    public AdvancedObservableCollection<TViewModel> View { get; } = [];

    public IncrementalLoadingCollection<FetchEngineIncrementalSource<T, TViewModel>, TViewModel> Source
    {
        get => (View.Source as IncrementalLoadingCollection<FetchEngineIncrementalSource<T, TViewModel>, TViewModel>)!;
        protected set => View.Source = value;
    }

    public IFetchEngine<T>? FetchEngine
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
            foreach (var entry in source)
                entry.Dispose();

        FetchEngine = null;
    }

    public void ResetEngine(IFetchEngine<T>? fetchEngine, int limit = -1)
    {
        Dispose();
        FetchEngine = fetchEngine;

        Source = new IncrementalLoadingCollection<FetchEngineIncrementalSource<T, TViewModel>, TViewModel>(FetchEngineIncrementalSource<T, TViewModel>.CreateInstance(FetchEngine!, limit));
    }
}
