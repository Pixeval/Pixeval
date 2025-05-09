// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Misaki;
using Pixeval.Database;

namespace Pixeval.Download.Models;

public interface IDownloadTaskGroup : IDownloadTaskBase, IIdentityInfo, INotifyPropertyChanged, INotifyPropertyChanging, IReadOnlyCollection<ImageDownloadTask>, IDisposable
{
    string IPlatformInfo.Platform => Pixiv;

    DownloadHistoryEntry DatabaseEntry { get; }

    ValueTask InitializeTaskGroupAsync();

    void SubscribeProgress(ChannelWriter<DownloadToken> writer);

    DownloadToken GetToken();

    int ActiveCount { get; }

    int CompletedCount { get; }

    int ErrorCount { get; }
}

public readonly record struct DownloadToken(IDownloadTaskGroup Task, CancellationToken Token);
