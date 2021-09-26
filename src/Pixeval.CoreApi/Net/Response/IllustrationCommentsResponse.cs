using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Pixeval.CoreApi.Model;

namespace Pixeval.CoreApi.Net.Response
{
    public class IllustrationCommentsResponse
    {
        [JsonPropertyName("comments")]
        public IEnumerable<Comment>? Comments { get; set; }

        [JsonPropertyName("next_url")]
        public string? NextUrl { get; set; }
    }
}