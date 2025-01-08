// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Model;

[Factory]
public partial record Tag
{
    [JsonPropertyName("name")]
    public required string Name { get; set; } = "";

    [JsonPropertyName("translated_name")]
    public required string? TranslatedName { get; set; }

    public string ToolTip => TranslatedName ?? Name;

    /// <summary>
    /// 好像只有小说会用这个属性
    /// </summary>
    [JsonPropertyName("added_by_uploaded_user")]
    public bool AddedByUploadedUser { get; set; } = false;
}
