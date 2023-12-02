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

    public DownloadHistoryEntry(DownloadState state, string? errorCause, string? destination, DownloadItemType type, string? id, string? url)
    {
        _state = state;
        _errorCause = errorCause;
        Destination = destination;
        Type = type;
        Id = id;
        Url = url;
    }

    // ReSharper disable once UnusedMember.Global
    public DownloadHistoryEntry()
    {
    }

    [BsonId(true)]
    public ObjectId? DownloadHistoryEntryId { get; set; }

    public string? Destination { get; set; }

    public DownloadItemType Type { get; set; }

    public string? Id { get; set; }

    public string? Url { get; set; }
}
