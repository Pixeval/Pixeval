// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Threading;
using Mako.Global.Enum;
using Misaki;

namespace Pixeval.Models.Database.Managers;

public interface IArtworkHistorySource
{
    event EventHandler? Changed;

    IAsyncEnumerable<IArtworkInfo> StreamAsync(SimpleWorkType workType, CancellationToken token = default);

    void Clear();
}
