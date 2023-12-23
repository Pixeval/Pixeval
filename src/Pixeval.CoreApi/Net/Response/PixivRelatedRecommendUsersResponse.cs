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
using System.Diagnostics;
using System.Text.Json.Serialization;
using Pixeval.CoreApi.Model;

namespace Pixeval.CoreApi.Net.Response;

public class PixivRelatedRecommendUsersResponse
{
    [JsonPropertyName("error")]
    public bool Error { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("body")]
    public Body? ResponseBody { get; set; }

    public class Body
    {
        [JsonPropertyName("recommendUsers")]
        public RecommendUser[]? RecommendUsers { get; set; }

        [JsonPropertyName("thumbnails")]
        public Thumbnails? Thumbnails { get; set; }

        [JsonPropertyName("users")]
        public User[]? Users { get; set; }
    }

    public class RecommendUser
    {
        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        [JsonPropertyName("illustIds")]
        public string[]? IllustIds { get; set; }

        [JsonPropertyName("novelIds")]
        public string[]? NovelIds { get; set; }
    }

    public class Thumbnails
    {
        [JsonPropertyName("illust")]
        public Illust[]? Illustrations { get; set; }

        [JsonPropertyName("novel")]
        public Novel[]? Novels { get; set; }
    }

    public class Illust
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("illustType")]
        public long? IllustType { get; set; }

        [JsonPropertyName("xRestrict")]
        public long? XRestrict { get; set; }

        [JsonPropertyName("restrict")]
        public long? Restrict { get; set; }

        [JsonPropertyName("sl")]
        public long Sl { get; set; }

        [JsonPropertyName("url")]
        public Uri? Url { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("tags")]
        public string[]? Tags { get; set; }

        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        [JsonPropertyName("userName")]
        public string? UserName { get; set; }

        [JsonPropertyName("width")]
        public long Width { get; set; }

        [JsonPropertyName("height")]
        public long Height { get; set; }

        [JsonPropertyName("pageCount")]
        public long PageCount { get; set; }

        [JsonPropertyName("isBookmarkable")]
        public bool IsBookmarkable { get; set; }

        [JsonPropertyName("alt")]
        public string? Alt { get; set; }

        [JsonPropertyName("createDate")]
        public string? CreateDate { get; set; }

        [JsonPropertyName("updateDate")]
        public string? UpdateDate { get; set; }

        [JsonPropertyName("isUnlisted")]
        public bool IsUnlisted { get; set; }

        [JsonPropertyName("isMasked")]
        public bool IsMasked { get; set; }

        [JsonPropertyName("urls")]
        public Urls? Urls { get; set; }

        [JsonPropertyName("profileImageUrl")]
        public Uri? ProfileImageUrl { get; set; }

        [JsonPropertyName("aiType")]
        public long AiType { get; set; }
    }

    public class Urls
    {
        [JsonPropertyName("250x250")]
        public string? The250X250 { get; set; }

        [JsonPropertyName("360x360")]
        public string? The360X360 { get; set; }

        [JsonPropertyName("540x540")]
        public string? The540X540 { get; set; }
    }

    public class Novel
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("xRestrict")]
        public long? XRestrict { get; set; }

        [JsonPropertyName("restrict")]
        public long Restrict { get; set; }

        [JsonPropertyName("url")]
        public Uri? Url { get; set; }

        [JsonPropertyName("tags")]
        public string[]? Tags { get; set; }

        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        [JsonPropertyName("userName")]
        public string? UserName { get; set; }

        [JsonPropertyName("profileImageUrl")]
        public Uri? ProfileImageUrl { get; set; }

        [JsonPropertyName("textCount")]
        public long TextCount { get; set; }

        [JsonPropertyName("wordCount")]
        public long WordCount { get; set; }

        [JsonPropertyName("readingTime")]
        public long ReadingTime { get; set; }

        [JsonPropertyName("useWordCount")]
        public bool UseWordCount { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("isBookmarkable")]
        public bool IsBookmarkable { get; set; }

        [JsonPropertyName("bookmarkCount")]
        public long BookmarkCount { get; set; }

        [JsonPropertyName("isOriginal")]
        public bool IsOriginal { get; set; }

        [JsonPropertyName("createDate")]
        public string? CreateDate { get; set; }

        [JsonPropertyName("updateDate")]
        public string? UpdateDate { get; set; }

        [JsonPropertyName("isMasked")]
        public bool IsMasked { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("seriesId")]
        public string? SeriesId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("seriesTitle")]
        public string? SeriesTitle { get; set; }

        [JsonPropertyName("isUnlisted")]
        public bool IsUnlisted { get; set; }

        [JsonPropertyName("aiType")]
        public long AiType { get; set; }
    }

    [DebuggerDisplay("{Id}: {Name}")]
    public class User : IIllustrate
    {
        [JsonPropertyName("partial")]
        public long Partial { get; set; }

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }

        [JsonPropertyName("followedBack")]
        public bool FollowedBack { get; set; }

        [JsonPropertyName("userId")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("image")]
        public string? Image { get; set; }

        [JsonPropertyName("imageBig")]
        public string? ImageBig { get; set; }

        [JsonPropertyName("premium")]
        public bool Premium { get; set; }

        [JsonPropertyName("isFollowed")]
        public bool IsFollowed { get; set; }

        [JsonPropertyName("isMypixiv")]
        public bool IsMypixiv { get; set; }

        [JsonPropertyName("isBlocking")]
        public bool IsBlocking { get; set; }

        [JsonPropertyName("acceptRequest")]
        public bool AcceptRequest { get; set; }
    }
}
