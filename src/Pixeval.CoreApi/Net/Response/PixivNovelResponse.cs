// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;
using Pixeval.CoreApi.Model;

namespace Pixeval.CoreApi.Net.Response;

[Factory]
public partial record PixivNovelResponse : IPixivNextUrlResponse<Novel>
{
    [JsonPropertyName("next_url")]
    public required string? NextUrl { get; set; }

    [JsonPropertyName("novels")]
    public /*override*/ required Novel[] Entities { get; set; } = [];
}
