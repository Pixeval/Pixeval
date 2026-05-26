// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using Mako.Model;
using Pixeval.Collections;

namespace Pixeval.ViewModels;

public class SimpleViewDataProvider<T, TViewModel> : ViewModelBase, IDataProvider<T, TViewModel>
    where T : class, IIdEntry
    where TViewModel : ViewModelBase
{
    public AdvancedObservableCollection<TViewModel> View { get; } = [];

    public IncrementalLoadingCollection<TViewModel> Source
    {
        get => (View.Source as IncrementalLoadingCollection<TViewModel>)!;
        protected set => View.Source = value;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        if (Source is { } source)
            foreach (var entry in source.OfType<IDisposable>())
                entry.Dispose();
    }

    public void ResetEngine(IAsyncEnumerable<T>? fetchEngine, Func<T, int, TViewModel> factory, int itemsPerPage = 20, int limit = -1)
    {
        Dispose();

        Source = new(new IncrementalSource<T,TViewModel>(fetchEngine!, factory, limit), itemsPerPage);
    }
}
