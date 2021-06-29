using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Model
{
    public record ProfileImageUrls
    {
        [JsonPropertyName("medium")]
        public string? Medium { get; set; }
    }
}