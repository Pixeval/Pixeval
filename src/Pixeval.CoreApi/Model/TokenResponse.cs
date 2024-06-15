#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/TokenResponse.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Text.Json.Serialization;
using Pixeval.CoreApi.Preference;

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

    public Session ToSession()
    {
        return new Session
        (
            User.Name,
            DateTime.Now + TimeSpan.FromSeconds(ExpiresIn) -
            TimeSpan.FromMinutes(5), // 减去5分钟是考虑到网络延迟会导致精确时间不可能恰好是一小时(TokenResponse的ExpireIn是60分钟)
            AccessToken,
            RefreshToken,
            User.ProfileImageUrls.Px170X170,
            User.Id,
            User.Account,
            User.IsPremium
        );
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
