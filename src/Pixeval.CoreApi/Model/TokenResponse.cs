// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Model;

/// <summary>
/// 这个类型比较特殊，并非网络请求的响应类型，因此放到Model而非Response目录中
/// </summary>
[Factory]
public partial record TokenResponse
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; } = "";

    [JsonPropertyName("expires_in")]
    public required long ExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public required string TokenType { get; set; } = "";

    [JsonPropertyName("scope")]
    public required string Scope { get; set; } = "";

    [JsonPropertyName("refresh_token")]
    public required string RefreshToken { get; set; } = "";

    [JsonPropertyName("user")]
    public required TokenUser User { get; set; }

    [JsonPropertyName("response")]
    public TokenResponse? Response { get; set; }

    public static TokenResponse CreateFromRefreshToken(string refreshToken)
    {
        var tokenResponse = CreateDefault();
        tokenResponse.RefreshToken = refreshToken;
        return tokenResponse;
    }
}

[Factory]
public partial record TokenUser
{
    [JsonPropertyName("profile_image_urls")]
    public required TokenProfileImageUrls ProfileImageUrls { get; set; }

    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public required long Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; } = "";

    [JsonPropertyName("account")]
    public required string Account { get; set; } = "";

    [JsonPropertyName("mail_address")]
    public required string MailAddress { get; set; } = "";

    [JsonPropertyName("is_premium")]
    public required bool IsPremium { get; set; }

    [JsonPropertyName("x_restrict")]
    public required long XRestrict { get; set; }

    [JsonPropertyName("is_mail_authorized")]
    public required bool IsMailAuthorized { get; set; }

    [JsonPropertyName("require_policy_agreement")]
    public required bool RequirePolicyAgreement { get; set; }
}

[Factory]
public partial record TokenProfileImageUrls
{
    [JsonPropertyName("px_16x16")]
    public required string Px16X16 { get; set; } = DefaultImageUrls.NoProfile;

    [JsonPropertyName("px_50x50")]
    public required string Px50X50 { get; set; } = DefaultImageUrls.NoProfile;

    [JsonPropertyName("px_170x170")]
    public required string Px170X170 { get; set; } = DefaultImageUrls.NoProfile;
}
