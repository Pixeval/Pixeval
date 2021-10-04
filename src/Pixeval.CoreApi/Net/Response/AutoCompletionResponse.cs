using System.Collections.Generic;
using System.Text.Json.Serialization;
using Pixeval.CoreApi.Model;

namespace Pixeval.CoreApi.Net.Response
{
    public class AutoCompletionResponse
    {
        [JsonPropertyName("tags")]
        public List<Tag>? Tags { get; set; }
    }
}