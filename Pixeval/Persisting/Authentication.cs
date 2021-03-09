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

using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Protocol;
using Pixeval.Data.Web.Request;
using Pixeval.Objects.Primitive;
using Refit;

namespace Pixeval.Persisting
{
    public class Authentication
    {
        private static readonly ITokenProtocol TokenProtocol = RestService.For<ITokenProtocol>(new HttpClient(PixivApiHttpClientHandler.Instance(true)) { BaseAddress = new Uri(HttpClientFactory.OAuthBaseUrl) });

        public static string GetCodeVer()
        {
            var bytes = new byte[32];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytes); // hope for an async version :(
            return bytes.ToUrlSafeBase64String();
        }

        private static string GetCodeChallenge(string code)
        {
            return code.HashBytes<SHA256CryptoServiceProvider>(Encoding.ASCII).ToUrlSafeBase64String();
        }

        public static string GenerateWebPageUrl(string codeVerify, bool signUp = false)
        {
            var codeChallenge = GetCodeChallenge(codeVerify);
            return signUp
                ? $"https://app-api.pixiv.net/web/v1/provisional-accounts/create?code_challenge={codeChallenge}&code_challenge_method=S256&client=pixiv-android"
                : $"https://app-api.pixiv.net/web/v1/login?code_challenge={codeChallenge}&code_challenge_method=S256&client=pixiv-android";
        }

        public static async Task AuthorizationCodeToToken(string code, string codeVerifier)
        {
            var token = await TokenProtocol.AuthorizationCodeToToken(
                new PasswordTokenRequest
                {
                    Code = code,
                    CodeVerifier = codeVerifier
                }
            );
            Session.Current = Session.Parse(token);
        }

        public static async Task Refresh(string refreshToken)
        { 
            var token = await TokenProtocol.RefreshToken(new RefreshTokenRequest { RefreshToken = refreshToken });
            Session.Current = Session.Parse(token);
        }
    }
}