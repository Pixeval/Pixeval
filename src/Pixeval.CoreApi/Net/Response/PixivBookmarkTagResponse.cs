// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;
using Pixeval.CoreApi.Model;

namespace Pixeval.CoreApi.Net.Response;

[Factory]
public partial record PixivBookmarkTagResponse : IPixivNextUrlResponse<BookmarkTag>
{
    [JsonPropertyName("next_url")]
    public required string? NextUrl { get; set; }

    [JsonPropertyName("bookmark_tags")]
    public /*override*/ required BookmarkTag[] Entities { get; set; } = [];
}
