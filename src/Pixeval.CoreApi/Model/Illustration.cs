#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/Illustration.cs
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

// ReSharper disable UnusedAutoPropertyAccessor.Global
public record Illustration : IIllustrate
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

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return Id.GetHashCode();
    }

    public virtual bool Equals(Illustration? other)
    {
        return other?.Id == Id;
    }


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

    public class MetaPage
    {
        [JsonPropertyName("image_urls")]
        public IllustrationImageUrls? ImageUrls { get; set; }
    }
}
