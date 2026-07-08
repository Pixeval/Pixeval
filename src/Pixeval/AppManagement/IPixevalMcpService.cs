// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.AppManagement;

public interface IPixevalMcpService : IAsyncDisposable
{
    Uri? Endpoint { get; }

    Task StartAsync(CancellationToken cancellationToken = default);

    Task ApplySettingsAsync(CancellationToken cancellationToken = default);
}
