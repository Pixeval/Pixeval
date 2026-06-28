using System.Text.Json.Serialization;

namespace Pixeval.Models.SauceNao;

public record SauceNaoResultHeader
{
    [JsonPropertyName("similarity")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public required double Similarity { get; set; }

    [JsonPropertyName("index_id")]
    public required SauceNaoIndex IndexId { get; set; }
}
