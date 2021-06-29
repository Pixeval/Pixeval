using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Net.Response
{
    
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    internal class UserSpecifiedBookmarkTagResponse
    {
        [JsonPropertyName("error")]
        public bool Error { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("body")]
        public Body? ResponseBody { get; set; }
        
        public class Body
        {
            [JsonPropertyName("public")]
            public IEnumerable<Tag>? Public { get; set; }

            [JsonPropertyName("private")]
            public IEnumerable<Tag>? Private { get; set; }

            [JsonPropertyName("tooManyBookmark")]
            public bool TooManyBookmark { get; set; }

            [JsonPropertyName("tooManyBookmarkTags")]
            public bool TooManyBookmarkTags { get; set; }
        }

        public class Tag
        {
            [JsonPropertyName("tag")]
            public string? Name { get; set; }

            [JsonPropertyName("cnt")]
            public long Count { get; set; }
        }
    }
}