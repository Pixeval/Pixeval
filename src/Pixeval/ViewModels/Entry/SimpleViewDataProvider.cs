// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Linq;
using Mako.Engine;
using Mako.Model;
using Pixeval.Collections;

namespace Pixeval.ViewModels;

public class SimpleViewDataProvider<T, TViewModel> : ViewModelBase, IDataProvider<T, TViewModel>
    where T : class, IIdEntry
    where TViewModel : class
{
    public AdvancedObservableCollection<TViewModel> View { get; } = [];

    public IncrementalLoadingCollection<FetchEngineIncrementalSource<T, TViewModel>, TViewModel> Source
    {
        get => (View.Source as IncrementalLoadingCollection<FetchEngineIncrementalSource<T, TViewModel>, TViewModel>)!;
        protected set => View.Source = value;
    }

    public IFetchEngine<T>? FetchEngine
    {
        get;
        protected set
        {
            if (value == field)
                return;
            field?.EngineHandle.Cancel();
            field = value;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        View.Dispose();
        if (Source is { } source)
            foreach (var entry in source.OfType<IDisposable>())
                entry.Dispose();

        FetchEngine = null;
    }

    public void ResetEngine(IFetchEngine<T>? fetchEngine, Func<T, int, TViewModel> factory, int itemsPerPage = 20, int limit = -1)
    {
        Dispose();
        FetchEngine = fetchEngine;

        Source = new(new(FetchEngine!, factory, limit), itemsPerPage);
    }
}
