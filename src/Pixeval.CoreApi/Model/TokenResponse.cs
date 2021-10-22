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

using System;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Pixeval.CoreApi.Preference;

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

        public Session ToSession()
        {
            return new Session
            {
                AccessToken = AccessToken,
                Account = User?.Account,
                AvatarUrl = User?.ProfileImageUrls?.Px170X170,
                ExpireIn = DateTime.Now + TimeSpan.FromSeconds(ExpiresIn) - TimeSpan.FromMinutes(5), // 减去5分钟是考虑到网络延迟会导致精确时间不可能恰好是一小时(TokenResponse的ExpireIn是60分钟)
                Id = User?.Id,
                IsPremium = User?.IsPremium ?? false,
                RefreshToken = RefreshToken,
                Name = User?.Name
            };
        }

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