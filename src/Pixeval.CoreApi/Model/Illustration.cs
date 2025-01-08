// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Model;

// ReSharper disable UnusedAutoPropertyAccessor.Global
[DebuggerDisplay("{Id}: {Title} [{User}]")]
[Factory]
public partial record Illustration : IWorkEntry
{
    [JsonPropertyName("id")]
    public required long Id { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; } = "";

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<IllustrationType>))]
    public required IllustrationType Type { get; set; }

    [JsonPropertyName("image_urls")]
    public required ImageUrls ThumbnailUrls { get; set; }

    [JsonPropertyName("caption")]
    public required string Caption { get; set; } = "";

    [JsonPropertyName("restrict")]
    [JsonConverter(typeof(BoolToNumberJsonConverter))]
    public required bool IsPrivate { get; set; }

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
    [EditorBrowsable(EditorBrowsableState.Never)]
    public required MetaSinglePage MetaSinglePage { get; set; }

    public string? OriginalSingleUrl => MetaSinglePage.OriginalImageUrl;

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

    [JsonPropertyName("illust_ai_type")]
    public required int AiType { get; set; }

    [JsonPropertyName("illust_book_style")]
    public required int IllustBookStyle { get; set; }

    [MemberNotNullWhen(true, nameof(OriginalSingleUrl))]
    public bool IsUgoira => Type is IllustrationType.Ugoira;

    [MemberNotNullWhen(false, nameof(OriginalSingleUrl))]
    public bool IsManga => PageCount > 1;

    public IReadOnlyList<string> MangaOriginalUrls => MetaPages.Select(m => m.ImageUrls.Original).ToArray();

    public List<string> GetUgoiraOriginalUrls(int frameCount)
    {
        Debug.Assert(IsUgoira);
        var list = new List<string>();
        for (var i = 0; i < frameCount; ++i)
            list.Add(OriginalSingleUrl.Replace("ugoira0", $"ugoira{i}"));
        return list;
    }

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
public partial record MetaSinglePage
{
    /// <summary>
    /// 单图或多图时的原图链接
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
}

[Factory]
public partial record MangaImageUrls : ImageUrls
{
    /// <summary>
    /// 多图时的原图链接
    /// </summary>
    [JsonPropertyName("original")]
    public required string Original { get; set; } = DefaultImageUrls.ImageNotAvailable;
}

[Factory]
public partial record MetaPage
{
    [JsonPropertyName("image_urls")]
    public required MangaImageUrls ImageUrls { get; set; }

    public string SquareMediumUrl => ImageUrls.SquareMedium;

    public string MediumUrl => ImageUrls.Medium;

    public string LargeUrl => ImageUrls.Large;

    /// <inheritdoc cref="MangaImageUrls.Original"/>
    public string OriginalUrl => ImageUrls.Original;
}

public enum XRestrict
{
    Ordinary = 0,
    R18 = 1,
    R18G = 2
}

public enum IllustrationType
{
    Illust,
    Manga,
    Ugoira
}
