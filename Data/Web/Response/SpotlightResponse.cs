using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Pixeval.Data.ViewModel;

namespace Pixeval.Data.Web.Response
{
    public class SpotlightResponse
    {
        [JsonProperty("spotlight_articles")]
        public List<SpotlightArticle> SpotlightArticles { get; set; }

        [JsonProperty("next_url")]
        public Uri NextUrl { get; set; }
    }
}