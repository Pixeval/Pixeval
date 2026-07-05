// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mako.Model;
using Pixeval.Collections;

namespace Pixeval.ViewModels;

public sealed class SimpleViewDataProvider<T, TViewModel> : ViewModelBase, IDataProvider<T, TViewModel>
    where T : class, IIdEntry
    where TViewModel : ViewModelBase
{
    private bool _isDisposed;

    public AdvancedObservableCollection<TViewModel> View { get; } = [];

    public ObservableCollection<TViewModel> Source
    {
        get => View.Source;
        private set => View.Source = value;
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        DisposeSourceItems();
        View.Dispose();
        _isDisposed = true;
    }

    public void ResetEngine(IAsyncEnumerable<T>? fetchEngine, Func<T, int, TViewModel> factory, int itemsPerPage = 20, int limit = -1)
    {
        DisposeSourceItems();

        Source = new IncrementalLoadingCollection<TViewModel>(new IncrementalSource<T,TViewModel>(fetchEngine!, factory, limit), itemsPerPage);
    }

    private void DisposeSourceItems()
    {
        if (Source is { } source)
            foreach (var entry in source.OfType<IDisposable>())
                entry.Dispose();
    }
}
