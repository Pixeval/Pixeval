#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/DownloadHistoryEntry.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using LiteDB;
using Pixeval.CoreApi.Model;
using Pixeval.Download;

namespace Pixeval.Database;

public partial class DownloadHistoryEntry : IHistoryEntry
{
    public DownloadHistoryEntry(string destination, DownloadItemType type, IWorkEntry entry)
    {
        Destination = destination;
        Type = type;
        Entry = entry;
    }

    // ReSharper disable once UnusedMember.Global
    public DownloadHistoryEntry()
    {
    }

    [BsonId(true)]
    public ObjectId? HistoryEntryId { get; set; }

    /// <summary>
    /// 此属性不会变为<see cref="DownloadState.Running"/>或<see cref="DownloadState.Paused"/>或<see cref="DownloadState.Pending"/>（而是由外部ViewModel使用）
    /// </summary>
    public DownloadState State { get; set; }

    /// <summary>
    /// 表示文件所在的地址，可能无法被直接解析，且不能包含未解析的宏@{...}，<br/>
    /// 当是一个文件时必须是一个有效的地址（不能是token&lt;...&gt;）<br/>
    /// 当是多个文件时，文件名可以包含token&lt;...&gt;，但其文件夹路径不能包含token&lt;...&gt;
    /// </summary>
    public string Destination { get; private set; } = null!;

    public DownloadItemType Type { get; set; }

    public IWorkEntry Entry { get; set; } = null!;
}
