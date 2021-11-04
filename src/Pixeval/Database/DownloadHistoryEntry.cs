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

using System;
using Pixeval.Download;
using SQLite;

namespace Pixeval.Database
{
    [Table("DownloadHistories")]
    public class DownloadHistoryEntry
    {
        public DownloadHistoryEntry(DownloadHistoryType type, IDownloadTask task, string id)
        {
            Type = type;
            Id = id;
            Title = task.Title;
            Description = task.Description;
            Url = task.Url;
            Destination = task.Destination;
            Thumbnail = task.Thumbnail;
            ErrorCause = task.ErrorCause?.ToString();
            UniqueCacheId = task.UniqueCacheId;
        }

        [PrimaryKey]
        [AutoIncrement]
        [Column("unique_id")]
        public Guid UniqueId { get; set; }

        [Column("type")]
        public DownloadHistoryType Type { get; set; }

        /// <summary>
        /// Id of the artwork, do not mix it with <see cref="UniqueId"/>, which is SQLite-compliant
        /// </summary>
        [Column("work_id")]
        public string? Id { get; set; }
        
        public string? Title { get; }

        public string? Description { get; }

        public string? Url { get; }

        public string? Destination { get; }

        public string? Thumbnail { get; }

        public string? ErrorCause { get; }

        /// <summary>
        /// See <see cref="IDownloadTask.UniqueCacheId"/>
        /// </summary>
        public string? UniqueCacheId { get; }
    }
}