// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System;
using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Model;

public record Spotlight : IIdEntry
{
    [JsonPropertyName("id")]
    public required long Id { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("pure_title")]
    public required string PureTitle { get; set; }

    [JsonPropertyName("thumbnail")]
    public required string Thumbnail { get; set; } = DefaultImageUrls.ImageNotAvailable;

    [JsonPropertyName("article_url")]
    public required string ArticleUrl { get; set; }

    [JsonPropertyName("publish_date")]
    public required DateTimeOffset PublishDate { get; set; }

    [JsonPropertyName("category")]
    [JsonConverter(typeof(JsonStringEnumConverter<SpotlightCategory>))]
    public required SpotlightCategory Category { get; set; }

    [JsonPropertyName("subcategory_label")]
    public required string SubcategoryLabel { get; set; }
}
