// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Misaki;
using Pixeval.Collections;
using Pixeval.Utilities;

namespace Pixeval.ViewModels;

/// <summary>
/// 复用时调用<see cref="CloneRef"/>，<see cref="EntrySourceRef"/>会在所有复用对象都Dispose时Dispose<br/>
/// 初始化时调用<see cref="ResetEngine"/>
/// </summary>
public class SharableViewDataProvider<T, TViewModel>
    : ViewModelBase, IDataProvider<T, TViewModel>
    where T : class, IIdentityInfo
    where TViewModel : EntryViewModel<T>
{
    private bool _isDisposed;

    protected SharedRef<IncrementalLoadingCollection<TViewModel>> EntrySourceRef
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

    public AdvancedObservableCollection<TViewModel> View { get; } = [];

    public ObservableCollection<TViewModel> Source => EntrySourceRef.Value;

    /// <summary>
    /// 多次释放可能导致崩溃
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_isDisposed)
            return;

        DisposeEntrySourceRef();
        View.Dispose();
        _isDisposed = true;
    }

    public void ResetEngine(IAsyncEnumerable<T>? fetchEngine, Func<T, int, TViewModel> factory, int itemsPerPage = 20, int limit = -1)
    {
        DisposeEntrySourceRef();

        EntrySourceRef = new(new(new IncrementalSource<T, TViewModel>(fetchEngine!, factory, limit), itemsPerPage), this);
    }

    public SharableViewDataProvider<T, TViewModel> CloneRef()
    {
        var dataProvider = new SharableViewDataProvider<T, TViewModel>();
        dataProvider.EntrySourceRef = EntrySourceRef.MakeShared(dataProvider);
        dataProvider.View.FilterCombinationMode = View.FilterCombinationMode;
        dataProvider.View.IsReversed = View.IsReversed;
        foreach (var viewSortDescription in View.SortDescriptions)
            dataProvider.View.SortDescriptions.Add(viewSortDescription);
        foreach (var viewFilter in View.Filters)
            dataProvider.View.Filters.Add(viewFilter);
        return dataProvider;
    }

    private void DisposeEntrySourceRef()
    {
        if (EntrySourceRef?.TryDispose(this) is true)
            foreach (var entry in Source.OfType<IDisposable>())
                entry.Dispose();
    }
}
