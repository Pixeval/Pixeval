#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/Illustration.cs
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
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Model;

// ReSharper disable UnusedAutoPropertyAccessor.Global
[DebuggerDisplay("{Id}: {Title} [{User}]")]
[Factory]
public partial record Illustration : IEntry
{
    [JsonIgnore]
    public bool FromSpotlight { get; set; }

    [JsonIgnore]
    public string? SpotlightTitle { get; set; }

    [JsonIgnore]
    public string? SpotlightId { get; set; }

    [JsonPropertyName("id")]
    public required long Id { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; } = "";

    [JsonPropertyName("type")]
    public required string Type { get; set; } = "";

    /// <summary>
    /// Original几乎总是null
    /// </summary>
    [JsonPropertyName("image_urls")]
    public required ImageUrls ImageUrls { get; set; }

    [JsonPropertyName("caption")]
    public required string Caption { get; set; } = "";

    [JsonPropertyName("restrict")]
    public required long Restrict { get; set; }

    [JsonPropertyName("user")]
    public required UserInfo User { get; set; }

    [JsonPropertyName("tags")]
    public required Tag[] Tags { get; set; } = [];

    [JsonPropertyName("tools")]
    public required string[] Tools { get; set; } = [];

    [JsonPropertyName("create_date")]
    public required DateTimeOffset CreateDate { get; set; }

    [JsonPropertyName("page_count")]
    public required long PageCount { get; set; }

    [JsonPropertyName("width")]
    public required int Width { get; set; }

    [JsonPropertyName("height")]
    public required int Height { get; set; }

    [JsonPropertyName("sanity_level")]
    public required long SanityLevel { get; set; }

    [JsonPropertyName("x_restrict")]
    public required XRestrict XRestrict { get; set; }

    [JsonPropertyName("meta_single_page")]
    public required IllustrationMetaSinglePage MetaSinglePage { get; set; }

    [JsonPropertyName("meta_pages")]
    public required MetaPage[] MetaPages { get; set; } = [];

    [JsonPropertyName("total_view")]
    public required int TotalView { get; set; }

    [JsonPropertyName("total_bookmarks")]
    public required int TotalBookmarks { get; set; }

    [JsonPropertyName("is_bookmarked")]
    public required bool IsBookmarked { get; set; }

    [JsonPropertyName("visible")]
    public required bool Visible { get; set; }

    [JsonPropertyName("is_muted")]
    public required bool IsMuted { get; set; }

    /// <summary>
    /// 值为2是AI生成
    /// </summary>
    [JsonPropertyName("illust_ai_type")]
    public required int AiType { get; set; }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return Id.GetHashCode();
    }

    public virtual bool Equals(Illustration? other)
    {
        return other?.Id == Id;
    }
}

[Factory]
public partial record IllustrationMetaSinglePage
{
    /// <summary>
    /// 单图时的原图链接
    /// </summary>
    [JsonPropertyName("original_image_url")]
    public string? OriginalImageUrl { get; set; } = DefaultImageUrls.ImageNotAvailable;
}

[Factory]
public partial record ImageUrls
{
    [JsonPropertyName("square_medium")]
    public required string SquareMedium { get; set; } = DefaultImageUrls.ImageNotAvailable;

    [JsonPropertyName("medium")]
    public required string Medium { get; set; } = DefaultImageUrls.ImageNotAvailable;

    [JsonPropertyName("large")]
    public required string Large { get; set; } = DefaultImageUrls.ImageNotAvailable;

    [JsonPropertyName("original")]
    public string? Original { get; set; } = DefaultImageUrls.ImageNotAvailable;
}

[Factory]
public partial record MetaPage
{
    /// <summary>
    /// 多图时的原图链接
    /// </summary>
    [JsonPropertyName("image_urls")]
    public required ImageUrls ImageUrls { get; set; }
}

public enum XRestrict
{
    Ordinary = 0,
    R18 = 1,
    R18G = 2
}
