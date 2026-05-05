// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.ComponentModel;
using Mako.Engine;
using Misaki;
using Pixeval.Collections;

namespace Pixeval.ViewModels;

public interface IDataProvider<T, TViewModel>
    : INotifyPropertyChanged, INotifyPropertyChanging, IDisposable
    where T : class, IIdentityInfo
    where TViewModel : class
{
    AdvancedObservableCollection<TViewModel> View { get; }

    IncrementalLoadingCollection<FetchEngineIncrementalSource<T, TViewModel>, TViewModel> Source { get; }

    IFetchEngine<T>? FetchEngine { get; }

    void ResetEngine(IFetchEngine<T>? fetchEngine, Func<T, int, TViewModel> factory, int itemsPerPage = 20, int limit = -1);
}
