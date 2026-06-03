// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using Misaki;
using Pixeval.Collections;

namespace Pixeval.ViewModels;

public interface IDataProvider<T, TViewModel>
    : ISourceView<TViewModel>
    where T : class, IIdentityInfo
    where TViewModel : class
{
    new AdvancedObservableCollection<TViewModel> View { get; }

    IAdvancedObservableView<TViewModel> ISourceView<TViewModel>.View => View;

    void ResetEngine(IAsyncEnumerable<T>? fetchEngine, Func<T, int, TViewModel> factory, int itemsPerPage = 20, int limit = -1);
}
