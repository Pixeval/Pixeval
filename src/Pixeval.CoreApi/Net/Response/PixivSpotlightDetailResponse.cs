// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Net.Response;

[Factory]
public partial record PixivSpotlightDetailResponse
{
    [JsonPropertyName("error")]
    public required bool Error { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; } = "";

    [JsonPropertyName("body")]
    public required SpotlightBody[] ResponseBody { get; set; } = [];
}

[Factory]
public partial record SpotlightBody
{
    [JsonPropertyName("id")]
    public required string Id { get; set; } = "";

    [JsonPropertyName("lang")]
    public required string Lang { get; set; } = "";

    [JsonPropertyName("entry")]
    public required Entry Entry { get; set; }

    [JsonPropertyName("tags")]
    public required PixivisionTag[] Tags { get; set; } = [];

    [JsonPropertyName("thumbnailUrl")]
    public required string ThumbnailUrl { get; set; } = "";

    [JsonPropertyName("title")]
    public required string Title { get; set; } = "";

    [JsonPropertyName("publishDate")]
    public required long PublishDate { get; set; }

    [JsonPropertyName("category")]
    public required string Category { get; set; } = "";

    [JsonPropertyName("subCategory")]
    public required string SubCategory { get; set; } = "";

    [JsonPropertyName("subCategoryLabel")]
    public required string SubCategoryLabel { get; set; } = "";

    [JsonPropertyName("subCategoryIntroduction")]
    public required string SubCategoryIntroduction { get; set; } = "";

    [JsonPropertyName("introduction")]
    public required string Introduction { get; set; } = "";

    [JsonPropertyName("footer")]
    public required string Footer { get; set; } = "";

    [JsonPropertyName("illusts")]
    public required Illust[] Illusts { get; set; } = [];

    [JsonPropertyName("relatedArticles")]
    public required RelatedArticle[] RelatedArticles { get; set; } = [];

    [JsonPropertyName("isOnlyOneUser")]
    public required bool IsOnlyOneUser { get; set; }
}

[Factory]
public partial record Entry
{
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public required long Id { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; } = "";

    [JsonPropertyName("pure_title")]
    public required string PureTitle { get; set; } = "";

    [JsonPropertyName("catchphrase")]
    public required string Catchphrase { get; set; } = "";

    [JsonPropertyName("header")]
    public required string Header { get; set; } = "";

    [JsonPropertyName("footer")]
    public required string Footer { get; set; } = "";

    [JsonPropertyName("sidebar")]
    public required string Sidebar { get; set; } = "";

    [JsonPropertyName("publish_date")]
    public required long PublishDate { get; set; }

    [JsonPropertyName("language")]
    public required string Language { get; set; } = "";

    [JsonPropertyName("pixivision_category_slug")]
    public required string PixivisionCategorySlug { get; set; } = "";

    [JsonPropertyName("pixivision_category")]
    public required PixivisionCategory PixivisionCategory { get; set; }

    [JsonPropertyName("pixivision_subcategory_slug")]
    public required string PixivisionSubcategorySlug { get; set; } = "";

    [JsonPropertyName("pixivision_subcategory")]
    public required PixivisionSubcategory PixivisionSubcategory { get; set; }

    [JsonPropertyName("tags")]
    public required PixivisionTag[] Tags { get; set; } = [];

    [JsonPropertyName("article_url")]
    public required string ArticleUrl { get; set; } = "";

    [JsonPropertyName("intro")]
    public required string Intro { get; set; } = "";

    [JsonPropertyName("facebook_count")]
    public required string FacebookCount { get; set; } = "";

    [JsonPropertyName("twitter_count")]
    public required string TwitterCount { get; set; } = "";
}

[Factory]
public partial record PixivisionCategory
{
    [JsonPropertyName("label")]
    public required string Label { get; set; } = "";

    [JsonPropertyName("introduction")]
    public required string Introduction { get; set; } = "";
}

[Factory]
public partial record PixivisionSubcategory
{
    [JsonPropertyName("label")]
    public required string Label { get; set; } = "";

    [JsonPropertyName("label_en")]
    public required string LabelEn { get; set; } = "";

    [JsonPropertyName("title")]
    public required string Title { get; set; } = "";

    [JsonPropertyName("introduction")]
    public required string Introduction { get; set; } = "";

    [JsonPropertyName("image_url")]
    public required string ImageUrl { get; set; } = "";

    [JsonPropertyName("big_image_url")]
    public required string BigImageUrl { get; set; } = "";
}

[Factory]
public partial record PixivisionTag
{
    [JsonPropertyName("id")]
    public required string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public required string Name { get; set; } = "";
}

[Factory]
public partial record Illust
{
    [JsonPropertyName("spotlight_article_id")]
    public required long SpotlightArticleId { get; set; }

    [JsonPropertyName("illust_id")]
    public required long IllustId { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; } = "";

    [JsonPropertyName("language")]
    public required string Language { get; set; } = "";

    [JsonPropertyName("illust_user_id")]
    public required string IllustUserId { get; set; } = "";

    [JsonPropertyName("illust_title")]
    public required string IllustTitle { get; set; } = "";

    [JsonPropertyName("illust_ext")]
    public required string IllustExt { get; set; } = "";

    [JsonPropertyName("illust_width")]
    public required string IllustWidth { get; set; } = "";

    [JsonPropertyName("illust_height")]
    public required string IllustHeight { get; set; } = "";

    [JsonPropertyName("illust_restrict")]
    public required string IllustRestrict { get; set; } = "";

    [JsonPropertyName("illust_x_restrict")]
    public required string IllustXRestrict { get; set; } = "";

    [JsonPropertyName("illust_create_date")]
    public required string IllustCreateDate { get; set; } = "";

    [JsonPropertyName("illust_upload_date")]
    public required string IllustUploadDate { get; set; } = "";

    [JsonPropertyName("illust_server_id")]
    public required string IllustServerId { get; set; } = "";

    [JsonPropertyName("illust_type")]
    public required string IllustType { get; set; } = "";

    [JsonPropertyName("illust_sanity_level")]
    public required long IllustSanityLevel { get; set; }

    [JsonPropertyName("illust_book_style")]
    public required string IllustBookStyle { get; set; } = "";

    [JsonPropertyName("illust_page_count")]
    public required string IllustPageCount { get; set; } = "";

    [JsonPropertyName("illust_custom_thumbnail_upload_datetime")]
    public required string IllustCustomThumbnailUploadDatetime { get; set; } = "";

    [JsonPropertyName("illust_comment")]
    public required string IllustComment { get; set; } = "";

    [JsonPropertyName("user_account")]
    public required string UserAccount { get; set; } = "";

    [JsonPropertyName("user_name")]
    public required string UserName { get; set; } = "";

    [JsonPropertyName("user_comment")]
    public required string UserComment { get; set; } = "";

    [JsonPropertyName("url")]
    public required Url Url { get; set; }

    [JsonPropertyName("user_icon")]
    public required string UserIcon { get; set; } = "";
}

[Factory]
public partial record Url
{
    [JsonPropertyName("1200x1200")]
    public required string The1200X1200 { get; set; } = "";

    [JsonPropertyName("768x1200")]
    public required string The768X1200 { get; set; } = "";

    [JsonPropertyName("ugoira600x600")]
    public required string Ugoira600X600 { get; set; } = "";

    [JsonPropertyName("ugoira1920x1080")]
    public required string Ugoira1920X1080 { get; set; } = "";
}

[Factory]
public partial record RelatedArticle
{
    [JsonPropertyName("id")]
    public required string Id { get; set; } = "";

    [JsonPropertyName("publish_date")]
    public required long PublishDate { get; set; }

    [JsonPropertyName("category")]
    public required string Category { get; set; } = "";

    [JsonPropertyName("pixivision_category_slug")]
    public required string PixivisionCategorySlug { get; set; } = "";

    [JsonPropertyName("pixivision_subcategory_slug")]
    public required string PixivisionSubcategorySlug { get; set; } = "";

    [JsonPropertyName("thumbnail")]
    public required string Thumbnail { get; set; } = "";

    [JsonPropertyName("thumbnail_illust_id")]
    public required string ThumbnailIllustId { get; set; } = "";

    [JsonPropertyName("has_body")]
    public required string HasBody { get; set; } = "";

    [JsonPropertyName("is_pr")]
    public required string IsPr { get; set; } = "";

    [JsonPropertyName("pr_client_name")]
    public required string PrClientName { get; set; } = "";

    [JsonPropertyName("edit_status")]
    public required string EditStatus { get; set; } = "";

    [JsonPropertyName("translation_status")]
    public required string TranslationStatus { get; set; } = "";

    [JsonPropertyName("is_sample")]
    public required string IsSample { get; set; } = "";

    [JsonPropertyName("memo")]
    public required string Memo { get; set; } = "";

    [JsonPropertyName("facebook_count")]
    public required string FacebookCount { get; set; } = "";

    [JsonPropertyName("tweet_count")]
    public required string TweetCount { get; set; } = "";

    [JsonPropertyName("tweet_max_count")]
    public required string TweetMaxCount { get; set; } = "";

    [JsonPropertyName("main_abtest_pattern_id")]
    public required string MainAbtestPatternId { get; set; } = "";

    [JsonPropertyName("advertisement_id")]
    public required string AdvertisementId { get; set; } = "";
}
