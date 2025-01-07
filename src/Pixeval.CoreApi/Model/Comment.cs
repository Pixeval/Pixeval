// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System;
using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Model;

[Factory]
public partial record Comment : IIdEntry
{
    [JsonPropertyName("id")]
    public required long Id { get; set; }

    [JsonPropertyName("comment")]
    public required string CommentContent { get; set; } = "";

    [JsonPropertyName("date")]
    public required DateTimeOffset Date { get; set; }

    [JsonPropertyName("user")]
    public required CommentUser CommentPoster { get; set; }

    [JsonPropertyName("has_replies")]
    public required bool HasReplies { get; set; }

    [JsonPropertyName("stamp")]
    public required Stamp? CommentStamp { get; set; }
}

[Factory]
public partial record Stamp
{
    [JsonPropertyName("stamp_id")]
    public required long StampId { get; set; }

    [JsonPropertyName("stamp_url")]
    public required string StampUrl { get; set; } = DefaultImageUrls.ImageNotAvailable;
}

[Factory]
public partial record CommentUser
{
    [JsonPropertyName("id")]
    public required long Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; } = "";

    [JsonPropertyName("account")]
    public required string Account { get; set; } = "";

    [JsonPropertyName("profile_image_urls")]
    public required ProfileImageUrls ProfileImageUrls { get; set; }
}
