// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;

namespace Mako.Net.Request;

public class ReverseSearchRequest(string apiKey)
{
    [JsonPropertyName("api_key")]
    public string ApiKey { get; } = apiKey;

    [JsonPropertyName("dbmask")]
    public string DbMask { get; } = "96";

    [JsonPropertyName("output_type")]
    public string OutputType { get; } = "2";

    [JsonPropertyName("numres")]
    public string NumberResult { get; } = "1";
}
