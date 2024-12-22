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
using System.Diagnostics.CodeAnalysis;
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
public partial class SharableViewDataProvider<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TViewModel>
    : ObservableObject, IDataProvider<T, TViewModel>, IDisposable
    where T : class, IIdEntry
    where TViewModel : EntryViewModel<T>, IFactory<T, TViewModel>, IDisposable
{
    public SharedRef<IFetchEngine<T>?>? FetchEngineRef
    {
        get;
        private set
        {
            if (Equals(value, field))
                return;
            if (field?.TryDispose(this) is true)
                FetchEngine?.EngineHandle.Cancel();
            field = value;
        }
    }

    protected SharedRef<IncrementalLoadingCollection<FetchEngineIncrementalSource<T, TViewModel>, TViewModel>> EntrySourceRef
    {
        get;
        set
        {
            if (Equals(field, value))
                return;

            DisposeEntrySourceRef();
            field = value;
            View.Source = value.Value;
            OnPropertyChanged();
        }
    } = null!;

    public IFetchEngine<T>? FetchEngine => FetchEngineRef?.Value;

    public AdvancedObservableCollection<TViewModel> View { get; } = [];

    public IncrementalLoadingCollection<FetchEngineIncrementalSource<T, TViewModel>, TViewModel> Source => EntrySourceRef.Value;

    /// <summary>
    /// 多次释放可能导致崩溃
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
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
        if (EntrySourceRef?.TryDispose(this) is true)
            foreach (var entryViewModel in Source)
                entryViewModel.Dispose();
    }
}
