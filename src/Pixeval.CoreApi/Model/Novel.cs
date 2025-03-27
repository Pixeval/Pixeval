// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mako.Model;

[Factory]
[DebuggerDisplay("{Id}: {Title} [{User}]")]
public partial record Novel : WorkBase, IWorkEntry
{
    [JsonPropertyName("restrict")]
    [JsonConverter(typeof(BoolToNumberJsonConverter))]
    public required bool IsPrivate { get; set; }

    [JsonPropertyName("is_original")]
    public required bool IsOriginal { get; set; }

    [JsonPropertyName("page_count")]
    public required int PageCount { get; set; }

    [JsonPropertyName("text_length")]
    public required int TextLength { get; set; }

    [JsonPropertyName("total_comments")]
    public required int TotalComments { get; set; }

    [JsonPropertyName("is_mypixiv_only")]
    public required bool IsMypixivOnly { get; set; }

    [JsonPropertyName("is_x_restricted")]
    public required bool IsXRestricted { get; set; }

    [JsonPropertyName("novel_ai_type")]
    public required AiType AiType { get; set; }
}

[Factory]
public partial record Series
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
