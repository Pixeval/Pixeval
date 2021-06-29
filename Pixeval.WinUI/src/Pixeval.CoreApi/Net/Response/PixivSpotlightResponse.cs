using System.Collections.Generic;
using System.Text.Json.Serialization;
using Pixeval.CoreApi.Model;

namespace Pixeval.CoreApi.Net.Response
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    internal class PixivSpotlightResponse
    {
        [JsonPropertyName("spotlight_articles")]
        public IEnumerable<SpotlightArticle>? SpotlightArticles { get; set; }

        [JsonPropertyName("next_url")]
        public string? NextUrl { get; set; }
    }
}