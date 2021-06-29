using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Net.Response
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    internal class PixivSpotlightDetailResponse
    {
        [JsonPropertyName("error")]
        public bool Error { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("body")]
        public IEnumerable<Body>? ResponseBody { get; set; }

        public class Body
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("lang")]
            public string? Lang { get; set; }

            [JsonPropertyName("entry")]
            public Entry? Entry { get; set; }

            [JsonPropertyName("tags")]
            public IEnumerable<Tag>? Tags { get; set; }

            [JsonPropertyName("thumbnailUrl")]
            public string? ThumbnailUrl { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("publishDate")]
            public long PublishDate { get; set; }

            [JsonPropertyName("category")]
            public string? Category { get; set; }

            [JsonPropertyName("subCategory")]
            public string? SubCategory { get; set; }

            [JsonPropertyName("subCategoryLabel")]
            public string? SubCategoryLabel { get; set; }

            [JsonPropertyName("subCategoryIntroduction")]
            public string? SubCategoryIntroduction { get; set; }

            [JsonPropertyName("introduction")]
            public string? Introduction { get; set; }

            [JsonPropertyName("footer")]
            public string? Footer { get; set; }

            [JsonPropertyName("illusts")]
            public IEnumerable<Illust>? Illusts { get; set; }

            [JsonPropertyName("relatedArticles")]
            public IEnumerable<RelatedArticle>? RelatedArticles { get; set; }

            [JsonPropertyName("isOnlyOneUser")]
            public bool? IsOnlyOneUser { get; set; }
        }

        public class Entry
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("pure_title")]
            public string? PureTitle { get; set; }

            [JsonPropertyName("catchphrase")]
            public string? Catchphrase { get; set; }

            [JsonPropertyName("header")]
            public string? Header { get; set; }

            [JsonPropertyName("footer")]
            public string? Footer { get; set; }

            [JsonPropertyName("sidebar")]
            public string? Sidebar { get; set; }

            [JsonPropertyName("publish_date")]
            public long PublishDate { get; set; }

            [JsonPropertyName("language")]
            public string? Language { get; set; }

            [JsonPropertyName("pixivision_category_slug")]
            public string? PixivisionCategorySlug { get; set; }

            [JsonPropertyName("pixivision_category")]
            public PixivisionCategory? PixivisionCategory { get; set; }

            [JsonPropertyName("pixivision_subcategory_slug")]
            public string? PixivisionSubcategorySlug { get; set; }

            [JsonPropertyName("pixivision_subcategory")]
            public PixivisionSubcategory? PixivisionSubcategory { get; set; }

            [JsonPropertyName("tags")]
            public IEnumerable<Tag>? Tags { get; set; }

            [JsonPropertyName("article_url")]
            public string? ArticleUrl { get; set; }

            [JsonPropertyName("intro")]
            public string? Intro { get; set; }

            [JsonPropertyName("facebook_count")]
            public string? FacebookCount { get; set; }

            [JsonPropertyName("twitter_count")]
            public string? TwitterCount { get; set; }
        }

        public class PixivisionCategory
        {
            [JsonPropertyName("label")]
            public string? Label { get; set; }

            [JsonPropertyName("introduction")]
            public string? Introduction { get; set; }
        }

        public class PixivisionSubcategory
        {
            [JsonPropertyName("label")]
            public string? Label { get; set; }

            [JsonPropertyName("label_en")]
            public string? LabelEn { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("introduction")]
            public string? Introduction { get; set; }

            [JsonPropertyName("image_url")]
            public string? ImageUrl { get; set; }

            [JsonPropertyName("big_image_url")]
            public string? BigImageUrl { get; set; }
        }

        public class Tag
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }
        }

        public class Illust
        {
            [JsonPropertyName("spotlight_article_id")]
            public long SpotlightArticleId { get; set; }

            [JsonPropertyName("illust_id")]
            public long IllustId { get; set; }

            [JsonPropertyName("description")]
            public string? Description { get; set; }

            [JsonPropertyName("language")]
            public string? Language { get; set; }

            [JsonPropertyName("illust_user_id")]
            public string? IllustUserId { get; set; }

            [JsonPropertyName("illust_title")]
            public string? IllustTitle { get; set; }

            [JsonPropertyName("illust_ext")]
            public string? IllustExt { get; set; }

            [JsonPropertyName("illust_width")]
            public string? IllustWidth { get; set; }

            [JsonPropertyName("illust_height")]
            public string? IllustHeight { get; set; }

            [JsonPropertyName("illust_restrict")]
            public string? IllustRestrict { get; set; }

            [JsonPropertyName("illust_x_restrict")]
            public string? IllustXRestrict { get; set; }

            [JsonPropertyName("illust_create_date")]
            public string? IllustCreateDate { get; set; }

            [JsonPropertyName("illust_upload_date")]
            public string? IllustUploadDate { get; set; }

            [JsonPropertyName("illust_server_id")]
            public string? IllustServerId { get; set; }

            [JsonPropertyName("illust_type")]
            public string? IllustType { get; set; }

            [JsonPropertyName("illust_sanity_level")]
            public long IllustSanityLevel { get; set; }

            [JsonPropertyName("illust_book_style")]
            public string? IllustBookStyle { get; set; }

            [JsonPropertyName("illust_page_count")]
            public string? IllustPageCount { get; set; }

            [JsonPropertyName("illust_custom_thumbnail_upload_datetime")]
            public string? IllustCustomThumbnailUploadDatetime { get; set; }

            [JsonPropertyName("illust_comment")]
            public string? IllustComment { get; set; }

            [JsonPropertyName("user_account")]
            public string? UserAccount { get; set; }

            [JsonPropertyName("user_name")]
            public string? UserName { get; set; }

            [JsonPropertyName("user_comment")]
            public string? UserComment { get; set; }

            [JsonPropertyName("url")]
            public Url? Url { get; set; }

            [JsonPropertyName("user_icon")]
            public Uri? UserIcon { get; set; }
        }

        public class Url
        {
            [JsonPropertyName("1200x1200")]
            public Uri? The1200X1200 { get; set; }

            [JsonPropertyName("768x1200")]
            public Uri? The768X1200 { get; set; }

            [JsonPropertyName("ugoira600x600")]
            public string? Ugoira600X600 { get; set; }

            [JsonPropertyName("ugoira1920x1080")]
            public string? Ugoira1920X1080 { get; set; }
        }

        public class RelatedArticle
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("publish_date")]
            public long? PublishDate { get; set; }

            [JsonPropertyName("category")]
            public string? Category { get; set; }

            [JsonPropertyName("pixivision_category_slug")]
            public string? PixivisionCategorySlug { get; set; }

            [JsonPropertyName("pixivision_subcategory_slug")]
            public string? PixivisionSubcategorySlug { get; set; }

            [JsonPropertyName("thumbnail")]
            public string? Thumbnail { get; set; }

            [JsonPropertyName("thumbnail_illust_id")]
            public string? ThumbnailIllustId { get; set; }

            [JsonPropertyName("has_body")]
            public string? HasBody { get; set; }

            [JsonPropertyName("is_pr")]
            public string? IsPr { get; set; }

            [JsonPropertyName("pr_client_name")]
            public string? PrClientName { get; set; }

            [JsonPropertyName("edit_status")]
            public string? EditStatus { get; set; }

            [JsonPropertyName("translation_status")]
            public string? TranslationStatus { get; set; }

            [JsonPropertyName("is_sample")]
            public string? IsSample { get; set; }

            [JsonPropertyName("memo")]
            public string? Memo { get; set; }

            [JsonPropertyName("facebook_count")]
            public string? FacebookCount { get; set; }

            [JsonPropertyName("tweet_count")]
            public string? TweetCount { get; set; }

            [JsonPropertyName("tweet_max_count")]
            public string? TweetMaxCount { get; set; }

            [JsonPropertyName("main_abtest_pattern_id")]
            public string? MainAbtestPatternId { get; set; }

            [JsonPropertyName("advertisement_id")]
            public string? AdvertisementId { get; set; }
        }
    }
}