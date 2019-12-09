using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pixeval.Data.Model.Web.Response
{
    public class IllustResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("response")]
        public List<Response> ToResponse { get; set; }

        [JsonProperty("count")]
        public long Count { get; set; }

        public class Response
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

            [JsonProperty("metadata")]
            public Metadata Metadata { get; set; }
        }

        public class ImageUrls
        {
            [JsonProperty("px_128x128")]
            public string Px128X128 { get; set; }

            [JsonProperty("small", NullValueHandling = NullValueHandling.Ignore)]
            public string Small { get; set; }

            [JsonProperty("medium")]
            public string Medium { get; set; }

            [JsonProperty("large")]
            public string Large { get; set; }

            [JsonProperty("px_480mw")]
            public string Px480Mw { get; set; }
        }

        public class Metadata
        {
            [JsonProperty("pages")]
            public List<Page> Pages { get; set; }
        }

        public class Page
        {
            [JsonProperty("image_urls")]
            public ImageUrls ImageUrls { get; set; }
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