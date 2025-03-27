// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;
using Mako.Model;

namespace Mako.Net.Response;

[Factory]
public partial record TrendingTagResponse
{
    [JsonPropertyName("trend_tags")]
    public required TrendingTag[] TrendTags { get; set; } = [];
}
