using System.Text.Json.Serialization;
using Pixeval.CoreApi.Models;

namespace Pixeval.CoreApi.Net.Responses
{
    public record RecommendationsResponse
    {
        [JsonPropertyName("illusts")]
        public Illustration[]? Illustrations { get; set; }
    }
}
