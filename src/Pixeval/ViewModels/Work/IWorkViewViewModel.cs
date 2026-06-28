// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using Misaki;

namespace Pixeval.ViewModels;

public interface IWorkViewViewModel : IOperableViewViewModel, IDisposable
{
    void ResetEngine(IAsyncEnumerable<IArtworkInfo>? newEngine, int itemsPerPage = 20, int itemLimit = -1);
}
