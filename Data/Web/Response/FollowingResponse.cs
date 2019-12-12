using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pixeval.Data.Web.Response
{
    public class FollowingResponse
    {
        [JsonProperty("user_previews")]
        public List<UserPreview> UserPreviews { get; set; }

        [JsonProperty("next_url")]
        public string NextUrl { get; set; }

        public class UserPreview
        {
            [JsonProperty("user")]
            public User User { get; set; }

            [JsonProperty("illusts")]
            public List<Illust> Illusts { get; set; }

            [JsonProperty("novels")]
            public List<Novel> Novels { get; set; }

            [JsonProperty("is_muted")]
            public bool IsMuted { get; set; }
        }

        public class Illust
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("image_urls")]
            public ImageUrls ImageUrls { get; set; }

            [JsonProperty("caption")]
            public string Caption { get; set; }

            [JsonProperty("restrict")]
            public long Restrict { get; set; }

            [JsonProperty("user")]
            public User User { get; set; }

            [JsonProperty("tags")]
            public List<IllustTag> Tags { get; set; }

            [JsonProperty("tools")]
            public List<string> Tools { get; set; }

            [JsonProperty("create_date")]
            public DateTimeOffset CreateDate { get; set; }

            [JsonProperty("page_count")]
            public long PageCount { get; set; }

            [JsonProperty("width")]
            public long Width { get; set; }

            [JsonProperty("height")]
            public long Height { get; set; }

            [JsonProperty("sanity_level")]
            public long SanityLevel { get; set; }

            [JsonProperty("x_restrict")]
            public long XRestrict { get; set; }

            [JsonProperty("series")]
            public Series Series { get; set; }

            [JsonProperty("meta_single_page")]
            public MetaSinglePage MetaSinglePage { get; set; }

            [JsonProperty("meta_pages")]
            public List<MetaPage> MetaPages { get; set; }

            [JsonProperty("total_view")]
            public long TotalView { get; set; }

            [JsonProperty("total_bookmarks")]
            public long TotalBookmarks { get; set; }

            [JsonProperty("is_bookmarked")]
            public bool IsBookmarked { get; set; }

            [JsonProperty("visible")]
            public bool Visible { get; set; }

            [JsonProperty("is_muted")]
            public bool IsMuted { get; set; }
        }

        public class ImageUrls
        {
            [JsonProperty("square_medium")]
            public string SquareMedium { get; set; }

            [JsonProperty("medium")]
            public string Medium { get; set; }

            [JsonProperty("large")]
            public string Large { get; set; }

            [JsonProperty("original", NullValueHandling = NullValueHandling.Ignore)]
            public Uri Original { get; set; }
        }

        public class MetaPage
        {
            [JsonProperty("image_urls")]
            public ImageUrls ImageUrls { get; set; }
        }

        public class MetaSinglePage
        {
            [JsonProperty("original_image_url", NullValueHandling = NullValueHandling.Ignore)]
            public Uri OriginalImageUrl { get; set; }
        }

        public class Series
        {
            [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
            public long? Id { get; set; }

            [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
            public string Title { get; set; }
        }

        public class IllustTag
        {
            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class User
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("account")]
            public string Account { get; set; }

            [JsonProperty("profile_image_urls")]
            public ProfileImageUrls ProfileImageUrls { get; set; }

            [JsonProperty("is_followed")]
            public bool IsFollowed { get; set; }
        }

        public class ProfileImageUrls
        {
            [JsonProperty("medium")]
            public string Medium { get; set; }
        }

        public class Novel
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("caption")]
            public string Caption { get; set; }

            [JsonProperty("restrict")]
            public long Restrict { get; set; }

            [JsonProperty("x_restrict")]
            public long XRestrict { get; set; }

            [JsonProperty("is_original")]
            public bool IsOriginal { get; set; }

            [JsonProperty("image_urls")]
            public ImageUrls ImageUrls { get; set; }

            [JsonProperty("create_date")]
            public DateTimeOffset CreateDate { get; set; }

            [JsonProperty("tags")]
            public List<NovelTag> Tags { get; set; }

            [JsonProperty("page_count")]
            public long PageCount { get; set; }

            [JsonProperty("text_length")]
            public long TextLength { get; set; }

            [JsonProperty("user")]
            public User User { get; set; }

            [JsonProperty("series")]
            public Series Series { get; set; }

            [JsonProperty("is_bookmarked")]
            public bool IsBookmarked { get; set; }

            [JsonProperty("total_bookmarks")]
            public long TotalBookmarks { get; set; }

            [JsonProperty("total_view")]
            public long TotalView { get; set; }

            [JsonProperty("visible")]
            public bool Visible { get; set; }

            [JsonProperty("total_comments")]
            public long TotalComments { get; set; }

            [JsonProperty("is_muted")]
            public bool IsMuted { get; set; }

            [JsonProperty("is_mypixiv_only")]
            public bool IsMypixivOnly { get; set; }

            [JsonProperty("is_x_restricted")]
            public bool IsXRestricted { get; set; }
        }

        public class NovelTag
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("added_by_uploaded_user")]
            public bool AddedByUploadedUser { get; set; }
        }
    }
}