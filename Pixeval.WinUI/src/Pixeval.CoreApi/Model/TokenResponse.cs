using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Pixeval.CoreApi.Model
{
    // 这个类型比较特殊，并非网络请求的响应类型，因此放到Model而非Response目录中
    [PublicAPI]
    public record TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("user")]
        public TokenUser? User { get; set; }

        [JsonPropertyName("response")]
        public TokenResponse? Response { get; set; }
        
        [PublicAPI]
        public class TokenUser
        {
            [JsonPropertyName("profile_image_urls")]
            public ProfileImageUrls? ProfileImageUrls { get; set; }

            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("account")]
            public string? Account { get; set; }

            [JsonPropertyName("mail_address")]
            public string? MailAddress { get; set; }

            [JsonPropertyName("is_premium")]
            public bool IsPremium { get; set; }

            [JsonPropertyName("x_restrict")]
            public long XRestrict { get; set; }

            [JsonPropertyName("is_mail_authorized")]
            public bool IsMailAuthorized { get; set; }

            [JsonPropertyName("require_policy_agreement")]
            public bool RequirePolicyAgreement { get; set; }
        }

        [PublicAPI]
        public class ProfileImageUrls
        {
            [JsonPropertyName("px_16x16")]
            public string? Px16X16 { get; set; }

            [JsonPropertyName("px_50x50")]
            public string? Px50X50 { get; set; }

            [JsonPropertyName("px_170x170")]
            public string? Px170X170 { get; set; }
        }
    }
}