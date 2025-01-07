// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Model;

[Factory]
public partial record TrendingTag
{
    [JsonPropertyName("tag")]
    public required string Tag { get; set; } = "";

    [JsonPropertyName("translated_name")]
    public required string TranslatedName { get; set; } = "";

    [JsonPropertyName("illust")]
    public required Illustration Illustration { get; set; }
}
