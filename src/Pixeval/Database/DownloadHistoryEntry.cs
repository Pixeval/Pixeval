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
        public static DownloadHistoryEntry Create(DownloadHistoryType type, IDownloadTask task, string id)
        {
            return new DownloadHistoryEntry
            {
                Type = type,
                Id = id,
                Title = task.Title,
                Description = task.Description,
                Url = task.Url,
                Destination = task.Destination,
                Thumbnail = task.Thumbnail,
                ErrorCause = task.ErrorCause?.ToString(),
                UniqueCacheId = task.UniqueCacheId,
            };
        }

        [PrimaryKey]
        [AutoIncrement]
        [Column("unique_id")]
        public Guid UniqueId { get; set; }

        [Column("type")]
        public DownloadHistoryType Type { get; set; }

        public DownloadState FinalState { get; set; }

        /// <summary>
        /// Id of the artwork, do not mix it with <see cref="UniqueId"/>, which is SQLite-compliant
        /// </summary>
        [Column("work_id")]
        public string? Id { get; set; }
        
        [Column("work_title")]
        public string? Title { get; set; }

        [Column("work_desc")]
        public string? Description { get; set; }

        [Column("work_url")]
        public string? Url { get; set; }

        [Column("destination")]
        public string? Destination { get; set; }

        [Column("work_thumbnail")]
        public string? Thumbnail { get; set; }

        [Column("work_error_cause")]
        public string? ErrorCause { get; set; }

        /// <summary>
        /// See <see cref="IDownloadTask.UniqueCacheId"/>
        /// </summary>
        [Column("work_unique_cache_id")]
        public string? UniqueCacheId { get; set; }
    }
}