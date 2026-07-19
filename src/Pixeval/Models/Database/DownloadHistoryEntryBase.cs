// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Misaki;
using Pixeval.Download;
using SQLite;

namespace Pixeval.Models.Database;

public abstract class DownloadHistoryEntryBase : ArtworkHistoryEntry
{
    protected DownloadHistoryEntryBase(string destination, IArtworkInfo entry) : base(entry)
    {
        Destination = destination;
    }

    protected DownloadHistoryEntryBase() : base(null)
    {
    }

    /// <summary>
    /// 数据库记录中，此属性不会变为<see cref="DownloadState.Running"/>或<see cref="DownloadState.Pending"/>（而是由外部ViewModel使用）
    /// </summary>
    public DownloadState State { get; set; }

    /// <inheritdoc cref="IDownloadTaskBase.Destination" />
    public abstract string Destination { get; init; }

    /// <summary>
    /// 此任务的格式令牌
    /// </summary>
    public string? FormatToken { get; set; }

    public string? ErrorMessage { get; set; }

    public static DownloadHistoryEntryBase Create(
        string destination,
        IArtworkInfo entry,
        int? workSubscriptionId = null) =>
        workSubscriptionId switch
        {
            null => new DownloadHistoryEntry(destination, entry),
            > 0 and var id => new SubscriptionDownloadHistoryEntry(destination, entry, id),
            _ => throw new ArgumentOutOfRangeException(nameof(workSubscriptionId))
        };

    [Ignore]
    public DownloadTaskKey DownloadTaskKey => this switch
    {
        DownloadHistoryEntry entry => DownloadTaskKey.CreateOrdinary(entry.Destination),
        SubscriptionDownloadHistoryEntry entry => DownloadTaskKey.CreateSubscription(
            entry.Destination,
            entry.WorkSubscriptionId,
            entry.ArtworkId),
        _ => throw new NotSupportedException(GetType().FullName)
    };
}
