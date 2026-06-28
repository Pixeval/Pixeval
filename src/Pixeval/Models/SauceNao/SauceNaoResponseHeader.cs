using System.Text.Json.Serialization;

namespace Pixeval.Models.SauceNao;

internal record SauceNaoResponseHeader
{
    [JsonPropertyName("user_id")]
    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public required int UserId { get; set; }

    [JsonPropertyName("status")]
    public required int Status { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
}
