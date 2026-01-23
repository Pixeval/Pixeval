// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Mako.Engine;
using Misaki;
using Pixeval.Collections;
using Pixeval.Controls;

namespace Pixeval.ViewModels;

public interface IDataProvider<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TViewModel>
    : INotifyPropertyChanged, INotifyPropertyChanging, IDisposable
    where T : class, IIdentityInfo
    where TViewModel : class, IFactory<T, TViewModel>
{
    AdvancedObservableCollection<TViewModel> View { get; }

    IncrementalLoadingCollection<FetchEngineIncrementalSource<T, TViewModel>, TViewModel> Source { get; }

    IFetchEngine<T>? FetchEngine { get; }

    void ResetEngine(IFetchEngine<T>? fetchEngine, int itemsPerPage = 20, int limit = -1);
}
