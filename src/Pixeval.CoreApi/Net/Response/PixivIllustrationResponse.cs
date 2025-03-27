// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;
using Mako.Model;

namespace Mako.Net.Response;

[Factory]
public partial record PixivIllustrationResponse : IPixivNextUrlResponse<Illustration>
{
    [JsonPropertyName("next_url")]
    public required string? NextUrl { get; set; }

    [JsonPropertyName("illusts")]
    public /*override*/ required Illustration[] Entities { get; set; } = [];
}
