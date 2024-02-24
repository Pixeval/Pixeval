#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/Novel.cs
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
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Model;

public record Novel
{
    [JsonPropertyName("id")]
    public required long Id { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("caption")]
    public required string Caption { get; set; }

    [JsonPropertyName("restrict")]
    public required long Restrict { get; set; }

    [JsonPropertyName("x_restrict")]
    public required long XRestrict { get; set; }

    [JsonPropertyName("is_original")]
    public required bool IsOriginal { get; set; }

    [JsonPropertyName("image_urls")]
    public required ImageUrls Cover { get; set; }

    [JsonPropertyName("create_date")]
    public required DateTimeOffset CreateDate { get; set; }

    [JsonPropertyName("tags")]
    public required List<Tag> Tags { get; set; }

    [JsonPropertyName("page_count")]
    public required long PageCount { get; set; }

    [JsonPropertyName("text_length")]
    public required long TextLength { get; set; }

    [JsonPropertyName("user")]
    public required UserInfo User { get; set; }

    [JsonPropertyName("series")]
    public required Series NovelSeries { get; set; }

    [JsonPropertyName("is_bookmarked")]
    public required bool IsBookmarked { get; set; }

    [JsonPropertyName("total_bookmarks")]
    public required long TotalBookmarks { get; set; }

    [JsonPropertyName("total_view")]
    public required long TotalView { get; set; }

    [JsonPropertyName("visible")]
    public required bool Visible { get; set; }

    [JsonPropertyName("total_comments")]
    public required long TotalComments { get; set; }

    [JsonPropertyName("is_muted")]
    public required bool IsMuted { get; set; }

    [JsonPropertyName("is_mypixiv_only")]
    public required bool IsMypixivOnly { get; set; }

    [JsonPropertyName("is_x_restricted")]
    public required bool IsXRestricted { get; set; }

    public class ImageUrls
    {
        [JsonPropertyName("square_medium")]
        public required string SquareMedium { get; set; }

        [JsonPropertyName("medium")]
        public required string Medium { get; set; }

        [JsonPropertyName("large")]
        public required string Large { get; set; }
    }

    public class Series
    {
        [JsonPropertyName("id")]
        public required long Id { get; set; }

        [JsonPropertyName("title")]
        public required string Title { get; set; }
    }

    public class Tag
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("translated_name")]
        public required string TranslatedName { get; set; }

        [JsonPropertyName("added_by_uploaded_user")]
        public required bool AddedByUploadedUser { get; set; }
    }

    public class UserInfo
    {
        [JsonPropertyName("id")]
        public required long Id { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("account")]
        public required string Account { get; set; }

        [JsonPropertyName("profile_image_urls")]
        public required ProfileImageUrls ProfileImageUrls { get; set; }

        [JsonPropertyName("is_followed")]
        public required bool IsFollowed { get; set; }
    }
}
