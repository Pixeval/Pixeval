// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

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
    /// 数据库记录中，此属性不会变为<see cref="DownloadState.Running"/>或<see cref="DownloadState.Paused"/>或<see cref="DownloadState.Pending"/>（而是由外部ViewModel使用）
    /// </summary>
    public DownloadState State { get; set; }

    /// <inheritdoc cref="IDownloadTaskBase.Destination" />
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    [Indexed(Unique = true)]
    public string Destination { get; init; } = null!;

    [Indexed]
    public int DownloadFolderId { get; set; }
}
