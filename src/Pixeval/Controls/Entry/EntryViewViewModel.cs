// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.Collections;
using Mako.Engine;
using Mako.Model;

namespace Pixeval.Controls;

public abstract class EntryViewViewModel<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TViewModel>
    : ObservableObject, IDisposable
    where T : class, IIdEntry
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

    public void ResetEngine(IFetchEngine<T>? newEngine, int itemsPerPage = 20, int itemLimit = -1) => DataProvider.ResetEngine(newEngine, itemsPerPage, itemLimit);

    /// <summary>
    /// 不用!<see cref="AdvancedObservableCollection{T}.HasMoreItems"/>，此处只是为了表示集合有没有元素
    /// </summary>
    public bool HasNoItem => DataProvider.View.Count is 0;
}
