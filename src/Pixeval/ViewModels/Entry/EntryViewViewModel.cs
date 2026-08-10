// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Misaki;
using Pixeval.Collections;
using Pixeval.Models.Blocking;

namespace Pixeval.ViewModels;

public abstract class EntryViewViewModel<T, TViewModel>
    : ViewModelBase, ISimpleViewViewModel, IDisposable
    where T : class, IIdentityInfo
    where TViewModel : ViewModelBase
{
    private bool _isDisposed;

    public abstract IDataProvider<T, TViewModel> DataProvider { get; }

    public AdvancedObservableCollection<TViewModel> View => DataProvider.View;

    public ObservableCollection<TViewModel> Source => DataProvider.Source;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_isDisposed)
            return;

        _isDisposed = true;
        DataProvider.Dispose();
    }

    public void ResetEngine(IAsyncEnumerable<T>? newEngine, Func<T, int, TViewModel> factory, int itemsPerPage = 20, int itemLimit = -1)
    {
        var snapshot = BlockedContentHelper.CaptureSnapshot();
        DataProvider.ResetEngine(
            newEngine,
            (entry, index) => factory(BlockedContentHelper.ReplaceEntry(entry, snapshot), index),
            itemsPerPage,
            itemLimit);
    }

    /// <inheritdoc />
    IReadOnlyCollection<INotifyPropertyChanged> ISimpleViewViewModel.View => View;

    /// <inheritdoc />
    IReadOnlyCollection<INotifyPropertyChanged> ISimpleViewViewModel.Source => Source;
}
