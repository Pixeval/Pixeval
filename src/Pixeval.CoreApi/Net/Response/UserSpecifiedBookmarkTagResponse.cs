// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;

namespace Mako.Net.Response;

[Factory]
public partial record UserSpecifiedBookmarkTagResponse
{
    [JsonPropertyName("error")]
    public required bool Error { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; } = "";

    [JsonPropertyName("body")]
    public required UserSpecifiedBookmarkTagBody ResponseBody { get; set; }
}

[Factory]
public partial record UserSpecifiedBookmarkTagBody
{
    [JsonPropertyName("public")]
    public required UserSpecifiedBookmarkTag[] Public { get; set; } = [];

    [JsonPropertyName("private")]
    public required UserSpecifiedBookmarkTag[] Private { get; set; } = [];

    [JsonPropertyName("tooManyBookmark")]
    public required bool TooManyBookmark { get; set; }

    [JsonPropertyName("tooManyBookmarkTags")]
    public required bool TooManyBookmarkTags { get; set; }
}

[Factory]
public partial record UserSpecifiedBookmarkTag
{
    [JsonPropertyName("tag")]
    public required string Name { get; set; } = "";

    [JsonPropertyName("cnt")]
    public required long Count { get; set; }
}
