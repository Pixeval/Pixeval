using System;
using Newtonsoft.Json;
using PropertyChanged;

namespace Pixeval.Data.Model.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class SpotlightArticle
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("pure_title")]
        public string PureTitle { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty("article_url")]
        public string ArticleUrl { get; set; }

        [JsonProperty("publish_date")]
        public DateTimeOffset PublishDate { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("subcategory_label")]
        public string SubcategoryLabel { get; set; }
    }
}