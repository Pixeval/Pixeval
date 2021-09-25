#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/Session.cs
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
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Preference
{
    [PublicAPI]
    public record Session
    {
        /// <summary>
        ///     User name
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        ///     Token expiration
        /// </summary>
        [JsonPropertyName("expireIn")]
        public DateTimeOffset ExpireIn { get; set; }

        /// <summary>
        ///     Current access token
        /// </summary>
        [JsonPropertyName("accessToken")]
        public string? AccessToken { get; set; }

        /// <summary>
        ///     Current refresh token
        /// </summary>
        [JsonPropertyName("refreshToken")]
        public string? RefreshToken { get; set; }

        /// <summary>
        ///     Avatar
        /// </summary>
        [JsonPropertyName("avatarUrl")]
        public string? AvatarUrl { get; set; }

        /// <summary>
        ///     User id
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        ///     Account for login
        /// </summary>
        [JsonPropertyName("account")]
        public string? Account { get; set; }

        /// <summary>
        ///     Indicates whether current user is Pixiv Premium or not
        /// </summary>
        [JsonPropertyName("isPremium")]
        public bool IsPremium { get; set; }

        /// <summary>
        ///     WebAPI cookie
        /// </summary>
        [JsonPropertyName("cookie")]
        public string? Cookie { get; set; }

        [JsonPropertyName("cookieCreation")]
        public DateTimeOffset CookieCreation { get; set; }

        public override string? ToString()
        {
            return this.ToJson();
        }
    }
}