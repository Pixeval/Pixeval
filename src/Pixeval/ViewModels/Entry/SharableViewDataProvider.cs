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
public sealed class SharableViewDataProvider<T, TViewModel>
    : ViewModelBase, IDataProvider<T, TViewModel>, IRefCloneable<SharableViewDataProvider<T, TViewModel>>
    where T : class, IIdentityInfo
    where TViewModel : EntryViewModel<T>
{
    private bool _isDisposed;

    private SharedRef<IncrementalLoadingCollection<TViewModel>>? _entrySourceRef;

    private SharedRef<IncrementalLoadingCollection<TViewModel>> EntrySourceRef
    {
        get => _entrySourceRef ?? throw new InvalidOperationException("The data provider has no entry source.");
        set
        {
            if (ReferenceEquals(_entrySourceRef, value))
                return;

            DisposeEntrySourceRef();
            _entrySourceRef = value;
            View.Source = value.Value;
            OnPropertyChanged();
        }
    }

    public AdvancedObservableCollection<TViewModel> View { get; } = [];

    public ObservableCollection<TViewModel> Source => EntrySourceRef.Value;

    /// <summary>
    /// 多次释放可能导致崩溃
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        DisposeEntrySourceRef();
        View.Dispose();
    }

    public void ResetEngine(IAsyncEnumerable<T>? fetchEngine, Func<T, int, TViewModel> factory, int itemsPerPage = 20, int limit = -1)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        DisposeEntrySourceRef();

        EntrySourceRef = new(new(new IncrementalSource<T, TViewModel>(fetchEngine!, factory, limit), itemsPerPage), this);
    }

    public SharableViewDataProvider<T, TViewModel> CloneRef()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
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
        if (_entrySourceRef is not { } entrySourceRef)
            return;

        var source = entrySourceRef.Value;
        View.Source = [];
        _entrySourceRef = null;
        if (entrySourceRef.TryDispose(this))
        {
            foreach (var entry in source.OfType<IDisposable>())
                entry.Dispose();
            source.Clear();
        }
    }
}
