// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Net.Response;

[Factory]
public partial record WebApiBookmarksWithTagResponse
{
    [JsonPropertyName("body")]
    public required WebApiBookmarksWithTagBody ResponseBody { get; set; }
}

[Factory]
public partial record WebApiBookmarksWithTagBody
{
    [JsonPropertyName("works")] 
    public required Work[] Works { get; set; } = [];
}

[Factory]
public partial record Work
{
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public required long Id { get; set; }
}
