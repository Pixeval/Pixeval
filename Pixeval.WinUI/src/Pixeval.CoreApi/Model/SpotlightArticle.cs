using System;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Pixeval.CoreApi.Model
{
    [PublicAPI]
    public record SpotlightArticle
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("thumbnail")]
        public string? Thumbnail { get; set; }

        [JsonPropertyName("article_url")]
        public string? ArticleUrl { get; set; }

        [JsonPropertyName("publish_date")]
        public DateTimeOffset PublishDate { get; set; }
    }
}