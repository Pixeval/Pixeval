// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Mako.Model;

[DebuggerDisplay("{UserInfo}")]
[Factory]
public partial record User : IIdEntry
{
    public long Id => UserInfo.Id;

    [JsonPropertyName("user")]
    public required UserInfo UserInfo { get; set; }

    [JsonPropertyName("illusts")]
    public required Illustration[] Illustrations { get; set; } = [];

    [JsonPropertyName("novels")]
    public required Novel[] Novels { get; set; } = [];

    [JsonPropertyName("is_muted")]
    public required bool IsMuted { get; set; }
}

[DebuggerDisplay("{Id}: {Name}")]
[Factory]
public partial record UserInfo
{
    [JsonPropertyName("id")]
    public required long Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; } = "";

    [JsonPropertyName("account")]
    public required string Account { get; set; } = "";

    public virtual string Description { get; set; } = "";

    [JsonPropertyName("profile_image_urls")]
    public required ProfileImageUrls ProfileImageUrls { get; set; }

    [JsonPropertyName("is_followed")]
    public bool IsFollowed { get; set; }
}
