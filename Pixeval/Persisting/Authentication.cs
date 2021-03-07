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
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Pixeval.Data.Web;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Protocol;
using Pixeval.Data.Web.Request;
using Pixeval.Objects.Exceptions;
using Pixeval.Objects.Generic;
using Pixeval.Objects.I18n;
using Pixeval.Objects.Primitive;
using Refit;

namespace Pixeval.Persisting
{
    public class Authentication
    {
        private const string ClientHash = "28c1fdd170a5204386cb1313c7077b34f83e4aaf4aa829ce78c231e05b0bae2c";

        private static string UtcTimeNow => DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss+00:00");

        public static string GetCodeVer()
        {
            var bytes = new byte[32];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytes); // hope for an async version :(
            return bytes.ToUrlSafeBase64String();
        }

        private static async Task<string> GetCodeChallenge(string code)
        {
            var bArr = Encoding.ASCII.GetBytes(code);
            var csp = new SHA256CryptoServiceProvider();
            await using var bStream = new MemoryStream(bArr);
            var resultBytes = csp.ComputeHash(bStream);
            return resultBytes.ToUrlSafeBase64String();
        }

        public static async Task<string> GenerateWebPageUrl(string codeVerify, bool signUp = false)
        {
            var codeChallenge = await GetCodeChallenge(codeVerify);
            return signUp
                ? $"https://app-api.pixiv.net/web/v1/provisional-accounts/create?code_challenge={codeChallenge}&code_challenge_method=S256&client=pixiv-android"
                : $"https://app-api.pixiv.net/web/v1/login?code_challenge={codeChallenge}&code_challenge_method=S256&client=pixiv-android";
        }

        public static async Task Authenticate(string name, string pwd)
        {
            var time = UtcTimeNow;
            var hash = (time + ClientHash).Hash<MD5CryptoServiceProvider>();

            try
            {
                var token = await RestService.For<ITokenProtocol>(HttpClientFactory.PixivApi(ProtocolBase.OAuthBaseUrl, true).Apply(h => h.Timeout = TimeSpan.FromSeconds(10))).GetTokenByPassword(new PasswordTokenRequest() /* TODO */, time, hash);
                Session.Current = Session.Parse(pwd, token);
            }
            catch (TaskCanceledException)
            {
                throw new AuthenticateFailedException(AkaI18N.AppApiAuthenticateTimeout);
            }
        }

        public static async Task AppApiAuthenticate(string refreshToken)
        {
            try
            {
                var token = await RestService.For<ITokenProtocol>(HttpClientFactory.PixivApi(ProtocolBase.OAuthBaseUrl, true).Apply(h => h.Timeout = TimeSpan.FromSeconds(10))).RefreshToken(new RefreshTokenRequest { RefreshToken = refreshToken });
                Session.Current = Session.Parse(Session.Current.Password, token);
            }
            catch (TaskCanceledException)
            {
                throw new AuthenticateFailedException(AkaI18N.AppApiAuthenticateTimeout);
            }
        }
    }
}