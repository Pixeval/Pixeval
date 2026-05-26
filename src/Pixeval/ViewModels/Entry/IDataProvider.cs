// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Misaki;
using Pixeval.Collections;

namespace Pixeval.ViewModels;

public interface IDataProvider<T, TViewModel>
    : INotifyPropertyChanged, INotifyPropertyChanging, IDisposable
    where T : class, IIdentityInfo
    where TViewModel : class
{
    AdvancedObservableCollection<TViewModel> View { get; }

    IncrementalLoadingCollection<TViewModel> Source { get; }

    void ResetEngine(IAsyncEnumerable<T>? fetchEngine, Func<T, int, TViewModel> factory, int itemsPerPage = 20, int limit = -1);
}
