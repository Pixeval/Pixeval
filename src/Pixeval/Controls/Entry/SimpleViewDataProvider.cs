// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Pixeval.Collections;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;

namespace Pixeval.Controls;

public partial class SimpleViewDataProvider<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TViewModel> : ObservableObject, IDataProvider<T, TViewModel>
    where T : class, IIdEntry
    where TViewModel : class, IFactory<T, TViewModel>, IDisposable
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
        if (Source is { } source)
            foreach (var entry in source)
                entry.Dispose();

        FetchEngine = null;
    }

    public void ResetEngine(IFetchEngine<T>? fetchEngine, int itemsPerPage = 20, int limit = -1)
    {
        Dispose();
        FetchEngine = fetchEngine;

        Source = new IncrementalLoadingCollection<FetchEngineIncrementalSource<T, TViewModel>, TViewModel>(FetchEngineIncrementalSource<T, TViewModel>.CreateInstance(FetchEngine!, limit), itemsPerPage);
    }
}
