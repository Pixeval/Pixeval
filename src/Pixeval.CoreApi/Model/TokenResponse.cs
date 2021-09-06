#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/TokenResponse.cs
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
            return new()
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