using System.Collections.Generic;
using System.Text.Json.Serialization;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Net.Response
{
    internal class WebApiBookmarksWithTagResponse
    {
        [JsonPropertyName("body")]
        public Body? ResponseBody { get; set; }
        
        public class Body
        {
            [JsonPropertyName("works")]
            public IEnumerable<Work>? Works { get; set; }
        }

        public class Work
        {
            [JsonPropertyName("id")]
            [JsonConverter(typeof(NumberOrStringToStringConverter))]
            public string? Id { get; set; }
        }
    }
}