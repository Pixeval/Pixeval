#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/PixivRelatedRecommendUsersResponse.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pixeval.CoreApi.Model;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Net.Response;

[Factory]
public partial record PixivRelatedRecommendUsersResponse
{
    [JsonPropertyName("error")]
    public required bool Error { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; } = "";

    [JsonPropertyName("body")]
    public required PixivRelatedRecommendUsersBody ResponseBody { get; set; }
}

[Factory]
public partial record PixivRelatedRecommendUsersBody
{
    [JsonPropertyName("recommendUsers")]
    public required RecommendMap[] RecommendMaps { get; set; } = [];

    [JsonPropertyName("thumbnails")]
    public required Thumbnails Thumbnails { get; set; }

    [JsonPropertyName("users")]
    public required RecommendUser[] Users { get; set; } = [];
}

[Factory]
public partial record RecommendMap
{
    [JsonPropertyName("userId")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public required long UserId { get; set; }

    [JsonPropertyName("illustIds")]
    [JsonConverter(typeof(StringArrayToNumberArrayConverter<long>))]
    public required long[] IllustIds { get; set; } = [];

    [JsonPropertyName("novelIds")]
    [JsonConverter(typeof(StringArrayToNumberArrayConverter<long>))]
    public required long[] NovelIds { get; set; } = [];
}

[Factory]
public partial record Thumbnails
{
    [JsonPropertyName("illust")]
    public required IllustProfile[] Illustrations { get; set; } = [];

    [JsonPropertyName("novel")]
    public required RecommendUserNovel[] Novels { get; set; } = [];
}

[Factory]
public partial record IllustProfile
{
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public required long Id { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; } = "";

    [JsonPropertyName("illustType")]
    public required IllustType IllustType { get; set; }

    [JsonPropertyName("xRestrict")]
    public required int XRestrict { get; set; }

    [JsonPropertyName("restrict")]
    public required int Restrict { get; set; }

    [JsonPropertyName("sl")]
    public required long Sl { get; set; }

    [JsonPropertyName("url")]
    public required string Url { get; set; } = "";

    [JsonPropertyName("description")]
    public required string Description { get; set; } = "";

    [JsonPropertyName("tags")]
    public required string[] Tags { get; set; } = [];

    [JsonPropertyName("userId")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public required long UserId { get; set; }

    [JsonPropertyName("userName")]
    public required string UserName { get; set; } = "";

    [JsonPropertyName("width")]
    public required int Width { get; set; }

    [JsonPropertyName("height")]
    public required int Height { get; set; }

    [JsonPropertyName("pageCount")]
    public required int PageCount { get; set; }

    [JsonPropertyName("isBookmarkable")]
    public required bool IsBookmarkable { get; set; }

    [JsonPropertyName("alt")]
    public required string Alt { get; set; } = "";

    [JsonPropertyName("createDate")]
    public required string CreateDate { get; set; } = "";

    [JsonPropertyName("updateDate")]
    public required string UpdateDate { get; set; } = "";

    [JsonPropertyName("isUnlisted")]
    public required bool IsUnlisted { get; set; }

    [JsonPropertyName("isMasked")]
    public required bool IsMasked { get; set; }

    [JsonPropertyName("urls")]
    public IllustProfileUrls? Urls { get; set; }

    [JsonPropertyName("profileImageUrl")]
    public required string ProfileImageUrl { get; set; } = "";

    [JsonPropertyName("aiType")]
    public required long AiType { get; set; }
}

[Factory]
public partial record IllustProfileUrls
{
    [JsonPropertyName("250x250")]
    public required string The250X250 { get; set; } = "";

    [JsonPropertyName("360x360")]
    public required string The360X360 { get; set; } = "";

    [JsonPropertyName("540x540")]
    public required string The540X540 { get; set; } = "";
}

[Factory]
public partial record RecommendUserNovel
{
    [JsonPropertyName("id")]
    public required string Id { get; set; } = "";

    [JsonPropertyName("title")]
    public required string Title { get; set; } = "";

    [JsonPropertyName("xRestrict")]
    public required long XRestrict { get; set; }

    [JsonPropertyName("restrict")]
    public required long Restrict { get; set; }

    [JsonPropertyName("url")]
    public required string Url { get; set; } = "";

    [JsonPropertyName("tags")]
    public required string[] Tags { get; set; } = [];

    [JsonPropertyName("userId")]
    public required string UserId { get; set; } = "";

    [JsonPropertyName("userName")]
    public required string UserName { get; set; } = "";

    [JsonPropertyName("profileImageUrl")]
    public required string ProfileImageUrl { get; set; } = "";

    [JsonPropertyName("textCount")]
    public required long TextCount { get; set; }

    [JsonPropertyName("wordCount")]
    public required long WordCount { get; set; }

    [JsonPropertyName("readingTime")]
    public required long ReadingTime { get; set; }

    [JsonPropertyName("useWordCount")]
    public required bool UseWordCount { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; } = "";

    [JsonPropertyName("isBookmarkable")]
    public required bool IsBookmarkable { get; set; }

    [JsonPropertyName("bookmarkCount")]
    public required long BookmarkCount { get; set; }

    [JsonPropertyName("isOriginal")]
    public required bool IsOriginal { get; set; }

    [JsonPropertyName("createDate")]
    public required string CreateDate { get; set; } = "";

    [JsonPropertyName("updateDate")]
    public required string UpdateDate { get; set; } = "";

    [JsonPropertyName("isMasked")]
    public required bool IsMasked { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("seriesId")]
    public string? SeriesId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("seriesTitle")]
    public string? SeriesTitle { get; set; }

    [JsonPropertyName("isUnlisted")]
    public required bool IsUnlisted { get; set; }

    [JsonPropertyName("aiType")]
    public required long AiType { get; set; }
}

[DebuggerDisplay("{Id}: {Name}")]
[Factory]
public partial record RecommendUser : IEntry
{
    [JsonPropertyName("partial")]
    public required long Partial { get; set; }

    [JsonPropertyName("comment")]
    public required string Comment { get; set; } = "";

    [JsonPropertyName("followedBack")]
    public required bool FollowedBack { get; set; }

    [JsonPropertyName("userId")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public required long Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; } = "";

    [JsonPropertyName("image")]
    public required string Image { get; set; } = DefaultImageUrls.NoProfile;

    [JsonPropertyName("imageBig")]
    public required string ImageBig { get; set; } = "";

    [JsonPropertyName("premium")]
    public required bool Premium { get; set; }

    [JsonPropertyName("isFollowed")]
    public required bool IsFollowed { get; set; }

    [JsonPropertyName("isMypixiv")]
    public required bool IsMypixiv { get; set; }

    [JsonPropertyName("isBlocking")]
    public required bool IsBlocking { get; set; }

    [JsonPropertyName("acceptRequest")]
    public required bool AcceptRequest { get; set; }
}

public enum IllustType
{
    Illustration,

    Manga,

    Animation
}

internal class StringArrayToNumberArrayConverter<T> : JsonConverter<T[]> where T : INumber<T>
{
    public override T[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
            return null;

        if (reader.TokenType is not JsonTokenType.StartArray)
            return ThrowUtils.Json<T[]>();

        var list = new List<T>();

        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndArray)
                return [.. list];

            if (reader.TokenType is not JsonTokenType.String)
                return ThrowUtils.Json<T[]>();

            var value = reader.GetString();

            if (value is null || !T.TryParse(value, null, out var v))
                return ThrowUtils.Json<T[]>();

            list.Add(v);
        }

        return ThrowUtils.Json<T[]>();
    }

    public override void Write(Utf8JsonWriter writer, T[]? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();

        foreach (var item in value)
        {
            writer.WriteStringValue(item.ToString());
        }

        writer.WriteEndArray();
    }
}
