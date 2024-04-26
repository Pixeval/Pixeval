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

using CommunityToolkit.Mvvm.ComponentModel;
using LiteDB;
using Pixeval.Download;

namespace Pixeval.Database;

public partial class DownloadHistoryEntry : ObservableObject
{
    [ObservableProperty]
    private string? _errorCause;

    [ObservableProperty]
    private DownloadState _state;

    public DownloadHistoryEntry(DownloadState state, string? destination, DownloadItemType type, long id)
    {
        _state = state;
        if (string.IsNullOrWhiteSpace(destination))
        {
            _state = DownloadState.Error;
            _errorCause += "No available destination.\n";
            Destination = "";
        }
        else
            Destination = destination;

        Type = type;
        Id = id;
    }

    // ReSharper disable once UnusedMember.Global
    public DownloadHistoryEntry()
    {
    }

    [BsonId(true)]
    public ObjectId? DownloadHistoryEntryId { get; set; }

    /// <summary>
    /// 表示文件所在的地址，可能无法被直接解析，且不能包含未解析的宏@{...}，<br/>
    /// 当是一个文件时必须是一个有效的地址（不能是token&lt;...&gt;）<br/>
    /// 当是多个文件时，文件名可以包含token&lt;...&gt;，但其文件夹路径不能包含token&lt;...&gt;
    /// </summary>
    public string Destination { get; set; } = null!;

    public DownloadItemType Type { get; set; }

    public long Id { get; set; }
}
