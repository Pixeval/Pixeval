#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.
// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Pixeval.Data.ViewModel;

namespace Pixeval.Data.Web.Response
{
    public class TaggedBookmarkResponse
    {
        [JsonProperty("error")]
        public bool Error { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("body")]
        public Body ResponseBody { get; set; }

        public class Body
        {
            [JsonProperty("works")]
            public Work[] Works { get; set; }

            [JsonProperty("total")]
            public long Total { get; set; }

            [JsonProperty("extraData")]
            public ExtraData ExtraData { get; set; }
        }

        public class ExtraData
        {
            [JsonProperty("meta")]
            public Meta Meta { get; set; }
        }

        public class Meta
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("canonical")]
            public Uri Canonical { get; set; }

            [JsonProperty("ogp")]
            public Ogp Ogp { get; set; }

            [JsonProperty("twitter")]
            public Ogp Twitter { get; set; }

            [JsonProperty("alternateLanguages")]
            public AlternateLanguages AlternateLanguages { get; set; }

            [JsonProperty("descriptionHeader")]
            public string DescriptionHeader { get; set; }
        }

        public class AlternateLanguages
        {
            [JsonProperty("ja")]
            public Uri Ja { get; set; }

            [JsonProperty("en")]
            public Uri En { get; set; }
        }

        public class Ogp
        {
            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("image")]
            public Uri Image { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
            public string Type { get; set; }

            [JsonProperty("card", NullValueHandling = NullValueHandling.Ignore)]
            public string Card { get; set; }
        }

        public class Work : IParser<Illustration>
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("illustType")]
            public long IllustType { get; set; }

            [JsonProperty("xRestrict")]
            public long XRestrict { get; set; }

            [JsonProperty("restrict")]
            public long Restrict { get; set; }

            [JsonProperty("sl")]
            public long Sl { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("tags")]
            public List<string> Tags { get; set; }

            [JsonProperty("userId")]
            public string UserId { get; set; }

            [JsonProperty("userName")]
            public string UserName { get; set; }

            [JsonProperty("width")]
            public long Width { get; set; }

            [JsonProperty("height")]
            public long Height { get; set; }

            [JsonProperty("pageCount")]
            public int PageCount { get; set; }

            [JsonProperty("isBookmarkable")]
            public bool IsBookmarkable { get; set; }

            [JsonProperty("bookmarkData")]
            public BookmarkData BookmarkData { get; set; }

            [JsonProperty("alt")]
            public string Alt { get; set; }

            [JsonProperty("titleCaptionTranslation")]
            public TitleCaptionTranslation TitleCaptionTranslation { get; set; }

            [JsonProperty("createDate")]
            public DateTimeOffset CreateDate { get; set; }

            [JsonProperty("updateDate")]
            public DateTimeOffset UpdateDate { get; set; }

            [JsonProperty("isUnlisted")]
            public bool IsUnlisted { get; set; }

            [JsonProperty("isMasked")]
            public bool IsMasked { get; set; }

            [JsonProperty("profileImageUrl")]
            public Uri ProfileImageUrl { get; set; }

            public Illustration Parse()
            {
                return new Illustration
                {
                    Id = Id,
                    IsLiked = BookmarkData != null,
                    IsManga = PageCount != 1,
                    IsUgoira = IllustType != 2,
                    Thumbnail = Url,
                    Tags = Tags.Select(t => new Tag { Name = t }),
                    PageCount = PageCount,
                    Incomplete = true
                };
            }
        }

        public class BookmarkData
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("private")]
            public bool Private { get; set; }
        }

        public class TitleCaptionTranslation
        {
            [JsonProperty("workTitle")]
            public string WorkTitle { get; set; }

            [JsonProperty("workCaption")]
            public string WorkCaption { get; set; }
        }
    }
}