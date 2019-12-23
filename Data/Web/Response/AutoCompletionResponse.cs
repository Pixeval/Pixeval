using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pixeval.Data.Web.Response
{
    public class AutoCompletionResponse
    {
        [JsonProperty("tags")]
        public List<Tag> Tags { get; set; }

        public class Tag
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("translated_name")]
            public string TranslatedName { get; set; }
        }
    }
}