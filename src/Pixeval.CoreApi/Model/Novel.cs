// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Model;

[Factory]
[DebuggerDisplay("{Id}: {Title} [{User}]")]
public partial record Novel : IWorkEntry
{
    [JsonPropertyName("id")]
    public required long Id { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; } = "";

    [JsonPropertyName("caption")]
    public required string Caption { get; set; } = "";

    [JsonPropertyName("restrict")]
    [JsonConverter(typeof(BoolToNumberJsonConverter))]
    public required bool IsPrivate { get; set; }

    [JsonPropertyName("x_restrict")]
    public required XRestrict XRestrict { get; set; }

    [JsonPropertyName("is_original")]
    public required bool IsOriginal { get; set; }

    [JsonPropertyName("image_urls")]
    public required ImageUrls ThumbnailUrls { get; set; }

    [JsonPropertyName("create_date")]
    public required DateTimeOffset CreateDate { get; set; }

    [JsonPropertyName("tags")]
    public required Tag[] Tags { get; set; } = [];

    [JsonPropertyName("page_count")]
    public required int PageCount { get; set; }

    [JsonPropertyName("text_length")]
    public required int TextLength { get; set; }

    [JsonPropertyName("user")]
    public required UserInfo User { get; set; }

    [JsonPropertyName("series")]
    public required Series NovelSeries { get; set; }

    [JsonPropertyName("is_bookmarked")]
    public required bool IsBookmarked { get; set; }

    [JsonPropertyName("total_bookmarks")]
    public required int TotalBookmarks { get; set; }

    [JsonPropertyName("total_view")]
    public required int TotalView { get; set; }

    [JsonPropertyName("visible")]
    public required bool Visible { get; set; }

    [JsonPropertyName("total_comments")]
    public required int TotalComments { get; set; }

    [JsonPropertyName("is_muted")]
    public required bool IsMuted { get; set; }

    [JsonPropertyName("is_mypixiv_only")]
    public required bool IsMypixivOnly { get; set; }

    [JsonPropertyName("is_x_restricted")]
    public required bool IsXRestricted { get; set; }

    [JsonPropertyName("novel_ai_type")]
    public required int AiType { get; set; }

    [JsonPropertyName("comment_access_control")]
    public int? CommentAccessControl { get; set; }
}

[Factory]
public partial class Series
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";
}

internal class BoolToNumberJsonConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var num = reader.GetInt32();
        return num is not 0;
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value ? 1 : 0);
    }
}
