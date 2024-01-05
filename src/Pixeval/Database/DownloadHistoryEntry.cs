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

using System.Collections.Generic;
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

    public DownloadHistoryEntry(DownloadState state, string? destination, DownloadItemType type, long id, params string?[] urls)
        : this(state, destination, type, id, (IEnumerable<string?>)urls)
    {
    }

    public DownloadHistoryEntry(DownloadState state, string? destination, DownloadItemType type, long id, IEnumerable<string?> urls)
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

        var set = new HashSet<string>();
        foreach (var url in urls)
            if (url is not null)
                if (set.Add(url))
                    Urls.Add(url);

        if (Urls.Count is 0)
        {
            _state = DownloadState.Error;
            _errorCause = "No available url.\n";
        }
    }

    // ReSharper disable once UnusedMember.Global
    public DownloadHistoryEntry()
    {
    }

    [BsonId(true)]
    public ObjectId? DownloadHistoryEntryId { get; set; }

    public string Destination { get; set; } = null!;

    public DownloadItemType Type { get; set; }

    public long Id { get; set; }

    public List<string> Urls { get; set; } = [];
}
