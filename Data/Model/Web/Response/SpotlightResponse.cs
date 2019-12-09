using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pixeval.Data.Model.Web.Response
{
    public class SpotlightResponse
    {
        [JsonProperty("spotlight_articles")]
        public List<SpotlightArticle> SpotlightArticles { get; set; }

        [JsonProperty("next_url")]
        public Uri NextUrl { get; set; }

        public class SpotlightArticle
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("pure_title")]
            public string PureTitle { get; set; }

            [JsonProperty("thumbnail")]
            public Uri Thumbnail { get; set; }

            [JsonProperty("article_url")]
            public Uri ArticleUrl { get; set; }

            [JsonProperty("publish_date")]
            public DateTimeOffset PublishDate { get; set; }

            [JsonProperty("category")]
            public string Category { get; set; }

            [JsonProperty("subcategory_label")]
            public string SubcategoryLabel { get; set; }
        }
    }
}