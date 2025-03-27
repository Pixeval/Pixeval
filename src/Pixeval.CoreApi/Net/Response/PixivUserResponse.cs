// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;
using Mako.Model;

namespace Mako.Net.Response;

[Factory]
public partial record PixivUserResponse : IPixivNextUrlResponse<User>
{
    [JsonPropertyName("next_url")]
    public required string? NextUrl { get; set; }

    [JsonPropertyName("user_previews")]
    public /*override*/ required User[] Entities { get; set; } = [];
}
