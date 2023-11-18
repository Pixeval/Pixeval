#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/Comment.cs
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
using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Model;

public class Comment
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("comment")]
    public string? CommentContent { get; set; }

    [JsonPropertyName("date")]
    public DateTimeOffset Date { get; set; }

    [JsonPropertyName("user")]
    public User? CommentPoster { get; set; }

    [JsonPropertyName("has_replies")]
    public bool HasReplies { get; set; }

    [JsonPropertyName("stamp")]
    public Stamp? CommentStamp { get; set; }

    public class Stamp
    {
        [JsonPropertyName("stamp_id")]
        public long StampId { get; set; }

        [JsonPropertyName("stamp_url")]
        public string? StampUrl { get; set; }
    }

    public class User
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("account")]
        public string? Account { get; set; }

        [JsonPropertyName("profile_image_urls")]
        public ProfileImageUrls? ProfileImageUrls { get; set; }
    }
}