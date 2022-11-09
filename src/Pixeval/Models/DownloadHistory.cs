#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/DownloadHistory.cs
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
using Pixeval.Data;

namespace Pixeval.Models;

public partial class DownloadHistory : ObservableObject
{
    [ObservableProperty] private string? _errorCause;


    //[ObservableProperty] private DownloadState _state;

    public DownloadHistory( string? errorCause, string? destination, DownloadItemType type,
        string? id, string? title, string? description, string? url, string? thumbnail)
    {
        _errorCause = errorCause;
        Destination = destination;
        Type = type;
        Id = id;
        Title = title;
        Description = description;
        Url = url;
        Thumbnail = thumbnail;
    }

    // ReSharper disable once UnusedMember.Global
    public DownloadHistory()
    {
    }

    [BsonId(true)] public ObjectId? DownloadHistoryEntryId { get; set; }

    public string? Destination { get; set; }

    public DownloadItemType Type { get; set; }

    public string? Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Url { get; set; }

    public string? Thumbnail { get; set; }
}

public enum DownloadItemType
{
    Manga,
    Ugoira,
    Illustration
}