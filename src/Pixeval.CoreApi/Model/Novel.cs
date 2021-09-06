#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/Novel.cs
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Model
{
    public record Novel
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("caption")]
        public string? Caption { get; set; }

        [JsonPropertyName("restrict")]
        public long Restrict { get; set; }

        [JsonPropertyName("x_restrict")]
        public long XRestrict { get; set; }

        [JsonPropertyName("is_original")]
        public bool IsOriginal { get; set; }

        [JsonPropertyName("image_urls")]
        public ImageUrls? Cover { get; set; }

        [JsonPropertyName("create_date")]
        public DateTimeOffset CreateDate { get; set; }

        [JsonPropertyName("tags")]
        public List<Tag>? Tags { get; set; }

        [JsonPropertyName("page_count")]
        public long PageCount { get; set; }

        [JsonPropertyName("text_length")]
        public long TextLength { get; set; }

        [JsonPropertyName("user")]
        public UserInfo? User { get; set; }

        [JsonPropertyName("series")]
        public Series? NovelSeries { get; set; }

        [JsonPropertyName("is_bookmarked")]
        public bool IsBookmarked { get; set; }

        [JsonPropertyName("total_bookmarks")]
        public long TotalBookmarks { get; set; }

        [JsonPropertyName("total_view")]
        public long TotalView { get; set; }

        [JsonPropertyName("visible")]
        public bool Visible { get; set; }

        [JsonPropertyName("total_comments")]
        public long TotalComments { get; set; }

        [JsonPropertyName("is_muted")]
        public bool IsMuted { get; set; }

        [JsonPropertyName("is_mypixiv_only")]
        public bool IsMypixivOnly { get; set; }

        [JsonPropertyName("is_x_restricted")]
        public bool IsXRestricted { get; set; }

        public class ImageUrls
        {
            [JsonPropertyName("square_medium")]
            public string? SquareMedium { get; set; }

            [JsonPropertyName("medium")]
            public string? Medium { get; set; }

            [JsonPropertyName("large")]
            public string? Large { get; set; }
        }

        public class Series
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }
        }

        public class Tag
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("translated_name")]
            public string? TranslatedName { get; set; }

            [JsonPropertyName("added_by_uploaded_user")]
            public bool AddedByUploadedUser { get; set; }
        }

        public class UserInfo
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("account")]
            public string? Account { get; set; }

            [JsonPropertyName("profile_image_urls")]
            public ProfileImageUrls? ProfileImageUrls { get; set; }

            [JsonPropertyName("is_followed")]
            public bool IsFollowed { get; set; }
        }
    }
}