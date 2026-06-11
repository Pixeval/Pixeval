// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Misaki;
using Pixeval.Download;
using SQLite;

namespace Pixeval.Models.Database;

public class DownloadHistoryEntry : ArtworkHistoryEntry
{
    public DownloadHistoryEntry(string destination, IArtworkInfo entry) : base(entry)
    {
        Destination = destination;
    }

    public DownloadHistoryEntry() : base(null)
    {
    }

    /// <summary>
    /// 数据库记录中，此属性不会变为<see cref="DownloadState.Running"/>或<see cref="DownloadState.Pending"/>（而是由外部ViewModel使用）
    /// </summary>
    public DownloadState State { get; set; }

    /// <inheritdoc cref="IDownloadTaskBase.Destination" />
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    [Indexed(Unique = true)]
    public string Destination { get; init; } = null!;

    /// <summary>
    /// 此任务的格式令牌
    /// </summary>
    public string? FormatToken { get; set; }

    public string? ErrorMessage { get; set; }

    [Indexed]
    public int WorkSubscriptionId { get; set; }
}
