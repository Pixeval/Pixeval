#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/EntryViewDataProvider.cs
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
using Pixeval.Utilities;

namespace Pixeval.Controls;

/// <summary>
/// 复用时调用<see cref="CloneRef"/>，<see cref="FetchEngineRef"/>和<see cref="EntrySourceRef"/>会在所有复用对象都Dispose时Dispose<br/>
/// 初始化时调用<see cref="ResetEngine"/>
/// </summary>
public class SharableViewDataProvider<T, TViewModel>
    : ObservableObject, IDataProvider<T, TViewModel>, IDisposable
    where T : class, IIdEntry
    where TViewModel : EntryViewModel<T>, IViewModelFactory<T, TViewModel>, IDisposable
{
    private SharedRef<IFetchEngine<T>?>? _fetchEngineRef;

    private SharedRef<IncrementalLoadingCollection<FetchEngineIncrementalSource<T, TViewModel>, TViewModel>> _entrySourceRef = null!;

    public SharedRef<IFetchEngine<T>?>? FetchEngineRef
    {
        get => _fetchEngineRef;
        private set
        {
            if (Equals(value, _fetchEngineRef))
                return;
            if (_fetchEngineRef?.TryDispose(this) is true)
                FetchEngine?.EngineHandle.Cancel();
            _fetchEngineRef = value;
        }
    }

    protected SharedRef<IncrementalLoadingCollection<FetchEngineIncrementalSource<T, TViewModel>, TViewModel>> EntrySourceRef
    {
        get => _entrySourceRef;
        set
        {
            if (Equals(_entrySourceRef, value))
                return;

            DisposeEntrySourceRef();
            _entrySourceRef = value;
            View.Source = value.Value;
            OnPropertyChanged();
        }
    }

    public IFetchEngine<T>? FetchEngine => _fetchEngineRef?.Value;

    public AdvancedObservableCollection<TViewModel> View { get; } = [];

    public IncrementalLoadingCollection<FetchEngineIncrementalSource<T, TViewModel>, TViewModel> Source => _entrySourceRef.Value;

    /// <summary>
    /// 多次释放可能导致崩溃
    /// </summary>
    public void Dispose()
    {
        DisposeEntrySourceRef();
        // 赋值为null会自动调用setter中的Dispose逻辑
        FetchEngineRef = null;
    }

    public void ResetEngine(IFetchEngine<T>? fetchEngine, int itemsPerPage = 20, int limit = -1)
    {
        FetchEngineRef = new SharedRef<IFetchEngine<T>?>(fetchEngine, this);
        EntrySourceRef = new SharedRef<IncrementalLoadingCollection<FetchEngineIncrementalSource<T, TViewModel>, TViewModel>>(new IncrementalLoadingCollection<FetchEngineIncrementalSource<T, TViewModel>, TViewModel>(FetchEngineIncrementalSource<T, TViewModel>.CreateInstance(FetchEngine!, limit), itemsPerPage), this);
    }

    public SharableViewDataProvider<T, TViewModel> CloneRef()
    {
        var dataProvider = new SharableViewDataProvider<T, TViewModel>();
        dataProvider.FetchEngineRef = FetchEngineRef?.MakeShared(dataProvider);
        dataProvider.EntrySourceRef = EntrySourceRef.MakeShared(dataProvider);
        // dataProvider.View.Filter = View.Filter;
        foreach (var viewSortDescription in View.SortDescriptions)
            dataProvider.View.SortDescriptions.Add(viewSortDescription);
        return dataProvider;
    }

    private void DisposeEntrySourceRef()
    {
        if (_entrySourceRef?.TryDispose(this) is true)
            foreach (var entryViewModel in Source)
                entryViewModel.Dispose();
    }
}
