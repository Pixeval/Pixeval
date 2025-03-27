// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;
using Mako.Model;

namespace Mako.Net.Response;

[Factory]
public partial record PixivNovelResponse : IPixivNextUrlResponse<Novel>
{
    [JsonPropertyName("next_url")]
    public required string? NextUrl { get; set; }

    [JsonPropertyName("novels")]
    public /*override*/ required Novel[] Entities { get; set; } = [];
}
