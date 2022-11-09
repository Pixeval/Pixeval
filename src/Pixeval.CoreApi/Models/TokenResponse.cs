#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/TokenResponse.cs
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

using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Models;

public record TokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; init; }

    [JsonPropertyName("expires_in")]
    public long ExpiresIn { get; init; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; init; }

    [JsonPropertyName("scope")]
    public string? Scope { get; init; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }

    [JsonPropertyName("user")]
    public TokenUser? User { get; init; }

    [JsonPropertyName("response")]
    public TokenResponse? Response { get; init; }
    
    public record TokenUser
    {
        [JsonPropertyName("profile_image_urls")]
        public ProfileImageUrls? ProfileImageUrls { get; init; }

        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("account")]
        public string? Account { get; init; }

        [JsonPropertyName("mail_address")]
        public string? MailAddress { get; init; }

        [JsonPropertyName("is_premium")]
        public bool IsPremium { get; init; }

        [JsonPropertyName("x_restrict")]
        public long XRestrict { get; init; }

        [JsonPropertyName("is_mail_authorized")]
        public bool IsMailAuthorized { get; init; }

        [JsonPropertyName("require_policy_agreement")]
        public bool RequirePolicyAgreement { get; init; }
    }
    
    public record ProfileImageUrls
    {
        [JsonPropertyName("px_16x16")]
        public string? Px16X16 { get; init; }

        [JsonPropertyName("px_50x50")]
        public string? Px50X50 { get; init; }

        [JsonPropertyName("px_170x170")]
        public string? Px170X170 { get; init; }
    }
}