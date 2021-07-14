using System.Text.Json.Serialization;

namespace Pixeval.LoginProxy
{
    public class LoginTokenResponse
    {
        [JsonPropertyName("errno")]
        public int Errno { get; set; }

        [JsonPropertyName("cookie")]
        public string? Cookie { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }
    }
}