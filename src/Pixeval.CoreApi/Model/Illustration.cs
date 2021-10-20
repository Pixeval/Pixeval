#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/Illustration.cs
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
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public record Illustration
    {
        [JsonIgnore]
        public bool FromSpotlight { get; set; }

        [JsonIgnore]
        public string? SpotlightTitle { get; set; }

        [JsonIgnore]
        public string? SpotlightId { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("image_urls")]
        public IllustrationImageUrls? ImageUrls { get; set; }

        [JsonPropertyName("caption")]
        public string? Caption { get; set; }

        [JsonPropertyName("restrict")]
        public long Restrict { get; set; }

        [JsonPropertyName("user")]
        public UserInfo? User { get; set; }

        [JsonPropertyName("tags")]
        public IEnumerable<Tag>? Tags { get; set; }

        [JsonPropertyName("tools")]
        public IEnumerable<string>? Tools { get; set; }

        [JsonPropertyName("create_date")]
        public DateTimeOffset CreateDate { get; set; }

        [JsonPropertyName("page_count")]
        public long PageCount { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("sanity_level")]
        public long SanityLevel { get; set; }

        [JsonPropertyName("x_restrict")]
        public long XRestrict { get; set; }

        [JsonPropertyName("meta_single_page")]
        public IllustrationMetaSinglePage? MetaSinglePage { get; set; }

        [JsonPropertyName("meta_pages")]
        public IEnumerable<MetaPage>? MetaPages { get; set; }

        [JsonPropertyName("total_view")]
        public int TotalView { get; set; }

        [JsonPropertyName("total_bookmarks")]
        public int TotalBookmarks { get; set; }

        [JsonPropertyName("is_bookmarked")]
        public bool IsBookmarked { get; set; }

        [JsonPropertyName("visible")]
        public bool Visible { get; set; }

        [JsonPropertyName("is_muted")]
        public bool IsMuted { get; set; }


        public class IllustrationMetaSinglePage
        {
            [JsonPropertyName("original_image_url")]
            public string? OriginalImageUrl { get; set; }
        }

        public class IllustrationImageUrls
        {
            [JsonPropertyName("square_medium")]
            public string? SquareMedium { get; set; }

            [JsonPropertyName("medium")]
            public string? Medium { get; set; }

            [JsonPropertyName("large")]
            public string? Large { get; set; }

            [JsonPropertyName("original")]
            public string? Original { get; set; }
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

        public class MetaPage
        {
            [JsonPropertyName("image_urls")]
            public IllustrationImageUrls? ImageUrls { get; set; }
        }
    }
}