#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/DownloadHistoryEntry.cs
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

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Pixeval.Download;
using SQLite;

namespace Pixeval.Database
{
    [Table("DownloadHistories")]
    public class DownloadHistoryEntry : ObservableObject
    {
        public DownloadHistoryEntry(DownloadState state, string? errorCause, string? destination, DownloadItemType type, string? id, string? title, string? description, string? url, string? thumbnail)
        {
            _state = state;
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
        public DownloadHistoryEntry()
        {

        }

        [PrimaryKey]
        [Column("destination")]
        public string? Destination { get; set; }

        [Column("type")]
        public DownloadItemType Type { get; set; }

        private DownloadState _state;

        [Column("state")]
        public DownloadState State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }

        [Column("work_id")]
        public string? Id { get; set; }

        [Column("work_title")]
        public string? Title { get; set; }

        [Column("work_desc")]
        public string? Description { get; set; }

        [Column("work_url")]
        public string? Url { get; set; }

        [Column("work_thumbnail")]
        public string? Thumbnail { get; set; }

        private string? _errorCause;

        [Column("work_error_cause")]
        public string? ErrorCause
        {
            get => _errorCause;
            set => SetProperty(ref _errorCause, value);
        }
    }
}