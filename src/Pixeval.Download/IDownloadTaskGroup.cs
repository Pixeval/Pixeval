// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Pixeval.Download;

public interface IDownloadTaskGroupBase : IDownloadTaskBase, INotifyPropertyChanged, INotifyPropertyChanging, IReadOnlyCollection<ISingleDownloadTaskBase>, IDisposable
{
    ValueTask InitializeTaskGroupAsync();

    void SubscribeProgress(ChannelWriter<DownloadToken> writer);

    DownloadToken GetToken();

    int ActiveCount { get; }

    int CompletedCount { get; }

    int ErrorCount { get; }
}

public readonly record struct DownloadToken(IDownloadTaskGroupBase Task, CancellationToken Token);
