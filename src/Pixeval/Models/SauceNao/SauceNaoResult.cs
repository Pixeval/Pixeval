using System.Text.Json.Serialization;

namespace Pixeval.Models.SauceNao;

public record SauceNaoResult
{
    [JsonPropertyName("header")]
    public required SauceNaoResultHeader Header { get; set; }

    [JsonPropertyName("data")]
    public required SauceNaoData Data { get; set; }
}
