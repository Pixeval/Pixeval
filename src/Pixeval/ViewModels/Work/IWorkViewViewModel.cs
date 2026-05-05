// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Mako.Engine;
using Misaki;

namespace Pixeval.ViewModels;

public interface IWorkViewViewModel : IOperableViewViewModel, IDisposable
{
    void ResetEngine(IFetchEngine<IArtworkInfo>? newEngine, bool isBookmarkEnabled = true, int itemsPerPage = 20, int itemLimit = -1);
}
