using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Model
{
    
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public static class IllustrationEssential
    {
        public record Illust
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("type")]
            public string? Type { get; set; }

            [JsonPropertyName("image_urls")]
            public ImageUrls? ImageUrls { get; set; }

            [JsonPropertyName("caption")]
            public string? Caption { get; set; }

            [JsonPropertyName("restrict")]
            public long Restrict { get; set; }

            [JsonPropertyName("user")]
            public User? User { get; set; }

            [JsonPropertyName("tags")]
            public IEnumerable<Tag>? Tags { get; set; }

            [JsonPropertyName("tools")]
            public IEnumerable<string>? Tools { get; set; }

            [JsonPropertyName("create_date")]
            public DateTimeOffset CreateDate { get; set; }

            [JsonPropertyName("page_count")]
            public long PageCount { get; set; }

            [JsonPropertyName("width")]
            public int Width { get; set; }

            [JsonPropertyName("height")]
            public int Height { get; set; }

            [JsonPropertyName("sanity_level")]
            public long SanityLevel { get; set; }

            [JsonPropertyName("x_restrict")]
            public long XRestrict { get; set; }

            [JsonPropertyName("meta_single_page")]
            public MetaSinglePage? MetaSinglePage { get; set; }

            [JsonPropertyName("meta_pages")]
            public IEnumerable<MetaPage>? MetaPages { get; set; }

            [JsonPropertyName("total_view")]
            public int TotalView { get; set; }

            [JsonPropertyName("total_bookmarks")]
            public int TotalBookmarks { get; set; }

            [JsonPropertyName("is_bookmarked")]
            public bool IsBookmarked { get; set; }

            [JsonPropertyName("visible")]
            public bool Visible { get; set; }

            [JsonPropertyName("is_muted")]
            public bool IsMuted { get; set; }
        }

        public class MetaSinglePage
        {
            [JsonPropertyName("original_image_url")]
            public string? OriginalImageUrl { get; set; }
        }

        public class ImageUrls
        {
            [JsonPropertyName("square_medium")]
            public string? SquareMedium { get; set; }

            [JsonPropertyName("medium")]
            public string? Medium { get; set; }

            [JsonPropertyName("large")]
            public string? Large { get; set; }

            [JsonPropertyName("original")]
            public string? Original { get; set; }
        }

        public class Tag
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("translated_name")]
            public string? TranslatedName { get; set; }
        }

        public class User
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("account")]
            public string? Account { get; set; }

            [JsonPropertyName("profile_image_urls")]
            public ProfileImageUrls? ProfileImageUrls { get; set; }

            [JsonPropertyName("is_followed")]
            public bool IsFollowed { get; set; }
        }

        public class MetaPage
        {
            [JsonPropertyName("image_urls")]
            public ImageUrls? ImageUrls { get; set; }
        }
    }
}