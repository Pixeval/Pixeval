// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Mako.Engine;
using Misaki;
using Pixeval.Controls;

namespace Pixeval.ViewModels;

public abstract class EntryViewViewModel<T, TViewModel>
    : ViewModelBase, IDisposable
    where T : class, IIdentityInfo
    where TViewModel : EntryViewModel<T>, IFactory<T, TViewModel>
{
    /// <summary>
    /// Avoid calls to <see cref="IDataProvider{T,TViewModel}.ResetEngine"/>, calls to <see cref="ResetEngine"/> instead.
    /// </summary>
    public abstract IDataProvider<T, TViewModel> DataProvider { get; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        DataProvider.Dispose();
    }

    public void ResetEngine(IFetchEngine<T>? newEngine, Func<T, int, TViewModel> factory, int itemsPerPage = 20, int itemLimit = -1) => DataProvider.ResetEngine(newEngine, factory, itemsPerPage, itemLimit);
}
