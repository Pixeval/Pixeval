#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using Newtonsoft.Json;

namespace Pixeval.Data.Web.Response
{
    public partial class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("user")]
        public Usr User { get; set; }

        [JsonProperty("response", NullValueHandling = NullValueHandling.Ignore)]
        public TokenResponse Response { get; set; }

        public class Usr
        {
            [JsonProperty("profile_image_urls")]
            public ProfileImageUrls ProfileImageUrls { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("account")]
            public string Account { get; set; }

            [JsonProperty("mail_address")]
            public string MailAddress { get; set; }

            [JsonProperty("is_premium")]
            public bool IsPremium { get; set; }

            [JsonProperty("x_restrict")]
            public long XRestrict { get; set; }

            [JsonProperty("is_mail_authorized")]
            public bool IsMailAuthorized { get; set; }

            [JsonProperty("require_policy_agreement")]
            public bool RequirePolicyAgreement { get; set; }
        }

        public class ProfileImageUrls
        {
            [JsonProperty("px_16x16")]
            public string Px16X16 { get; set; }

            [JsonProperty("px_50x50")]
            public string Px50X50 { get; set; }

            [JsonProperty("px_170x170")]
            public string Px170X170 { get; set; }
        }
    }
}