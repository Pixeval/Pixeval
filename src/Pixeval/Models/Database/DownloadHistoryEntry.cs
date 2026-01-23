// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Misaki;
using Pixeval.Download;

namespace Pixeval.Models.Database;

public class DownloadHistoryEntry : ArtworkHistoryEntry
{
    public DownloadHistoryEntry(string destination, DownloadItemType type, IArtworkInfo entry) : base(entry)
    {
        Destination = destination;
        Type = type;
    }

    public DownloadHistoryEntry() : base(null)
    {
    }

    /// <summary>
    /// 此属性不会变为<see cref="DownloadState.Running"/>或<see cref="DownloadState.Paused"/>或<see cref="DownloadState.Pending"/>（而是由外部ViewModel使用）
    /// </summary>
    public DownloadState State { get; set; }

    /// <summary>
    /// 表示文件所在的地址，可能无法被直接解析，且不能包含未解析的宏@{...}，<br/>
    /// 当是一个文件时必须是一个有效的地址（不能是token&lt;...&gt;）<br/>
    /// 当是多个文件时，文件名可以包含token&lt;...&gt;，但其文件夹路径不能包含token&lt;...&gt;
    /// </summary>
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public string Destination { get; init; } = null!;

    public DownloadItemType Type { get; set; }
}
