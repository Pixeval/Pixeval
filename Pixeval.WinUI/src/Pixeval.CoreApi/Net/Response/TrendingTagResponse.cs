using System.Collections.Generic;
using System.Text.Json.Serialization;
using Pixeval.CoreApi.Model;

namespace Pixeval.CoreApi.Net.Response
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    internal class TrendingTagResponse
    {
        [JsonPropertyName("trend_tags")]
        public IEnumerable<TrendTag>? TrendTags { get; set; }

        public class TrendTag
        {
            [JsonPropertyName("tag")]
            public string? TagStr { get; set; }

            [JsonPropertyName("translated_name")]
            public string? TranslatedName { get; set; }

            [JsonPropertyName("illust")]
            public IllustrationEssential.Illust? Illust { get; set; }
        }
    }
}