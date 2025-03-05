// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Model;

[Factory]
public partial record NovelContent
{
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public required long Id { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; } = "";

    [JsonPropertyName("seriesId")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public required long? SeriesId { get; set; }

    [JsonPropertyName("seriesTitle")]
    public required string? SeriesTitle { get; set; } = "";

    [JsonPropertyName("seriesIsWatched")]
    public required bool? SeriesIsWatched { get; set; }

    [JsonPropertyName("userId")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public required long UserId { get; set; }

    [JsonPropertyName("coverUrl")]
    public required string CoverUrl { get; set; } = "";

    [JsonPropertyName("tags")]
    public required string[] Tags { get; set; } = [];

    [JsonPropertyName("caption")]
    public required string Caption { get; set; } = "";

    [JsonPropertyName("cdate")]
    public required DateTimeOffset Date { get; set; }

    [JsonPropertyName("rating")]
    public required Rating Rating { get; set; }

    [JsonPropertyName("text")]
    public required string Text { get; set; } = "";

    [JsonPropertyName("marker")]
    public string? Marker { get; set; }

    /// <summary>
    /// 可以在主站找到原图的插图
    /// </summary>
    [JsonPropertyName("illusts")]
    [JsonConverter(typeof(SpecialDictionaryConverter<NovelIllustInfo>))]
    public required NovelIllustInfo[] Illusts { get; set; } = [];

    /// <summary>
    /// 临时上传的，没有ID的小说插图
    /// </summary>
    /// <remarks>
    /// key: <see cref="NovelImage.NovelImageId"/>
    /// </remarks>
    [JsonPropertyName("images")]
    [JsonConverter(typeof(SpecialDictionaryConverter<NovelImage>))]
    public required NovelImage[] Images { get; set; } = [];

    [JsonPropertyName("seriesNavigation")]
    public required SeriesNavigation? SeriesNavigation { get; set; }

    [JsonPropertyName("glossaryItems")]
    public required NovelReplaceableGlossary[] GlossaryItems { get; set; } = [];

    [JsonPropertyName("replaceableItemIds")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public required long[] ReplaceableItemIds { get; set; } = [];

    [JsonPropertyName("aiType")]
    public required int AiType { get; set; }

    [JsonPropertyName("isOriginal")]
    public required bool IsOriginal { get; set; }
}

[Factory]
public partial record Rating
{
    [JsonPropertyName("like")]
    public required int Like { get; set; }

    [JsonPropertyName("bookmark")]
    public required int Bookmark { get; set; }

    [JsonPropertyName("view")]
    public required int View { get; set; }
}

[Factory]
public partial record SeriesNavigation
{
    [JsonPropertyName("nextNovel")]
    public NovelNavigation? NextNovel { get; set; }

    [JsonPropertyName("prevNovel")]
    public NovelNavigation? PrevNovel { get; set; }
}

[Factory]
public partial record NovelNavigation
{
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public required long Id { get; set; }

    [JsonPropertyName("viewable")]
    public required bool Viewable { get; set; }

    [JsonPropertyName("contentOrder")]
    public required string ContentOrder { get; set; } = "";

    [JsonPropertyName("title")]
    public required string Title { get; set; } = "";

    [JsonPropertyName("coverUrl")]
    public required string CoverUrl { get; set; } = DefaultImageUrls.ImageNotAvailable;

    [JsonPropertyName("viewableMessage")]
    public string? ViewableMessage { get; set; }
}

[Factory]
public partial record NovelImage
{
    [JsonPropertyName("novelImageId")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public required long NovelImageId { get; set; }

    [JsonPropertyName("sl")]
    public required string Sl { get; set; } = "";

    [JsonPropertyName("urls")]
    public required NovelImageUrls Urls { get; set; }

    public string ThumbnailUrl => Urls.X1200;

    public string OriginalUrl => Urls.Original;
}

[Factory]
public partial record NovelImageUrls
{
    /// <summary>
    /// Max width 240
    /// </summary>
    [JsonPropertyName("240mw")]
    public required string Mw240 { get; set; } = DefaultImageUrls.ImageNotAvailable;

    /// <summary>
    /// Max width 480
    /// </summary>
    [JsonPropertyName("480mw")]
    public required string Mw480 { get; set; } = DefaultImageUrls.ImageNotAvailable;

    [JsonPropertyName("1200x1200")]
    public required string X1200 { get; set; } = DefaultImageUrls.ImageNotAvailable;

    [JsonPropertyName("128x128")]
    public required string X128 { get; set; } = DefaultImageUrls.ImageNotAvailable;

    [JsonPropertyName("original")]
    public required string Original { get; set; } = DefaultImageUrls.ImageNotAvailable;
}

[Factory]
public partial record NovelIllustInfo : IIdEntry
{
    [JsonPropertyName("visible")]
    public required bool Visible { get; set; }

    [JsonPropertyName("availableMessage")]
    public required string? AvailableMessage { get; set; }

    [JsonPropertyName("illust")]
    public required NovelIllust Illust { get; set; }

    [JsonPropertyName("user")]
    public required NovelUser User { get; set; }

    /// <summary>
    /// 相当于<see cref="Illustration.Id"/>
    /// </summary>
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public required long Id { get; set; }

    /// <summary>
    /// 表示当漫画时，在漫画的页数，从 1 开始
    /// </summary>
    [JsonPropertyName("page")]
    public required int Page { get; set; } = 1;

    public string ThumbnailUrl => Illust.Images.Medium;
}

[Factory]
public partial record NovelIllust
{
    [JsonPropertyName("title")]
    public required string Title { get; set; } = "";

    [JsonPropertyName("description")]
    public required string Description { get; set; } = "";

    [JsonPropertyName("restrict")]
    public required int Restrict { get; set; }

    [JsonPropertyName("xRestrict")]
    public required int XRestrict { get; set; }

    [JsonPropertyName("sl")]
    public required int Sl { get; set; }

    [JsonPropertyName("tags")]
    public required NovelTag[] Tags { get; set; } = [];

    [JsonPropertyName("images")]
    public required NovelIllustUrls Images { get; set; }
}

[Factory]
public partial record NovelTag
{
    [JsonPropertyName("tag")]
    public required string Tag { get; set; } = "";

    [JsonPropertyName("userId")]
    public required string UserId { get; set; } = "";
}

[Factory]
public partial record NovelIllustUrls
{
    [JsonPropertyName("small")]
    public required string? Small { get; set; }

    [JsonPropertyName("medium")]
    public required string Medium { get; set; } = DefaultImageUrls.ImageNotAvailable;

    [JsonPropertyName("original")]
    public required string? Original { get; set; }
}

[Factory]
public partial record NovelUser
{
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public required long Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; } = "";

    [JsonPropertyName("image")]
    public required string Image { get; set; } = DefaultImageUrls.NoProfile;
}

[Factory]
public partial record NovelReplaceableGlossary
{
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public required long Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; } = "";

    [JsonPropertyName("overview")]
    public required string Overview { get; set; } = "";

    [JsonPropertyName("coverImage")]
    public required string Cover { get; set; } = DefaultImageUrls.ImageNotAvailable;
}

/// <summary>
/// 当为空对象时，表现为空数组。
/// 当为正常对象时，属性键被包含在属性的值中，故直接抛弃。
/// </summary>
/// <typeparam name="T"></typeparam>
internal class SpecialDictionaryConverter<T> : JsonConverter<T[]>
{
    public override T[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
            return null;

        if (reader.TokenType is JsonTokenType.StartArray && reader.Read() && reader.TokenType is JsonTokenType.EndArray)
            return [];

        var list = new List<T>();

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    continue;
                case JsonTokenType.PropertyName when !reader.Read():
                    return ThrowUtils.Json<T[]>();
                case JsonTokenType.PropertyName:
                {
                    var propertyValue = (T) JsonSerializer.Deserialize(ref reader, typeof(T), AppJsonSerializerContext.Default)!;
                    list.Add(propertyValue);
                    break;
                }
                case JsonTokenType.EndObject:
                    return [.. list];
            }
        }

        return ThrowUtils.Json<T[]>();
    }

    public override void Write(Utf8JsonWriter writer, T[]? value, JsonSerializerOptions options) => ThrowUtils.NotSupported();
}
