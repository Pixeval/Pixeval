// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
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

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Pixeval.Core;
using Pixeval.Data.ViewModel;
using Pixeval.Objects;

namespace Pixeval.Data.Web.Response
{
    public class UploadResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("response")]
        public List<Response> ToResponse { get; set; }

        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("pagination")]
        public Pagination Pages { get; set; }

        public class Pagination
        {
            [JsonProperty("previous", NullValueHandling = NullValueHandling.Include)]
            public long? Previous { get; set; }

            [JsonProperty("next", NullValueHandling = NullValueHandling.Include)]
            public long? Next { get; set; }

            [JsonProperty("current")]
            public long Current { get; set; }

            [JsonProperty("per_page")]
            public long PerPage { get; set; }

            [JsonProperty("total")]
            public long Total { get; set; }

            [JsonProperty("pages")]
            public long Pages { get; set; }
        }

        public class Response : IParser<Illustration>
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("caption")]
            public string Caption { get; set; }

            [JsonProperty("tags")]
            public List<string> Tags { get; set; }

            [JsonProperty("tools")]
            public List<string> Tools { get; set; }

            [JsonProperty("image_urls")]
            public ImageUrls ImageUrls { get; set; }

            [JsonProperty("width")]
            public long Width { get; set; }

            [JsonProperty("height")]
            public long Height { get; set; }

            [JsonProperty("stats")]
            public Stats Stats { get; set; }

            [JsonProperty("publicity")]
            public long Publicity { get; set; }

            [JsonProperty("age_limit")]
            public string AgeLimit { get; set; }

            [JsonProperty("created_time")]
            public DateTimeOffset CreatedTime { get; set; }

            [JsonProperty("reuploaded_time")]
            public DateTimeOffset ReuploadedTime { get; set; }

            [JsonProperty("user")]
            public User User { get; set; }

            [JsonProperty("is_manga")]
            public bool IsManga { get; set; }

            [JsonProperty("is_liked")]
            public bool IsLiked { get; set; }

            [JsonProperty("favorite_id")]
            public long FavoriteId { get; set; }

            [JsonProperty("page_count")]
            public long PageCount { get; set; }

            [JsonProperty("book_style")]
            public string BookStyle { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("sanity_level")]
            public string SanityLevel { get; set; }

            public Illustration Parse()
            {
                return new Illustration
                {
                    Bookmark = (int) (Stats.FavoritedCount.Public + Stats.FavoritedCount.Private),
                    Id = Id.ToString(),
                    IsLiked = FavoriteId != 0,
                    IsUgoira = Type == "ugoira",
                    IsManga = IsManga,
                    Origin = ImageUrls.Large,
                    Large = ImageUrls.Large,
                    Tags = Tags.Select(t => new Tag {Name = t}),
                    Thumbnail = ImageUrls.Px480Mw.IsNullOrEmpty() ? ImageUrls.Px128X128 : ImageUrls.Px480Mw,
                    Title = Title,
                    UserId = User.Id.ToString(),
                    UserName = User.Name,
                    Resolution = $"{Width}x{Height}",
                    ViewCount = (int) Stats.ViewsCount,
                    PublishDate = CreatedTime
                }.Apply(async i =>
                {
                    if (i.IsManga) i.MangaMetadata = (await PixivHelper.IllustrationInfo(i.Id)).MangaMetadata;
                });
            }
        }

        public class ImageUrls
        {
            [JsonProperty("px_128x128")]
            public string Px128X128 { get; set; }

            [JsonProperty("px_480mw")]
            public string Px480Mw { get; set; }

            [JsonProperty("large")]
            public string Large { get; set; }
        }

        public class Stats
        {
            [JsonProperty("scored_count")]
            public long ScoredCount { get; set; }

            [JsonProperty("score")]
            public long Score { get; set; }

            [JsonProperty("views_count")]
            public long ViewsCount { get; set; }

            [JsonProperty("favorited_count")]
            public FavoritedCount FavoritedCount { get; set; }

            [JsonProperty("commented_count")]
            public long CommentedCount { get; set; }
        }

        public class FavoritedCount
        {
            [JsonProperty("public")]
            public long Public { get; set; }

            [JsonProperty("private")]
            public long Private { get; set; }
        }

        public class User
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("account")]
            public string Account { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("is_following")]
            public bool IsFollowing { get; set; }

            [JsonProperty("is_follower")]
            public bool IsFollower { get; set; }

            [JsonProperty("is_friend")]
            public bool IsFriend { get; set; }

            [JsonProperty("profile_image_urls")]
            public ProfileImageUrls ProfileImageUrls { get; set; }
        }

        public class ProfileImageUrls
        {
            [JsonProperty("px_50x50")]
            public string Px50X50 { get; set; }
        }
    }
}