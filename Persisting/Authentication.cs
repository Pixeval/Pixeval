// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
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

using System;
using System.Threading.Tasks;
using Pixeval.Data.Web;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Protocol;
using Pixeval.Data.Web.Request;
using Pixeval.Objects;
using Pixeval.Objects.Exceptions;
using Refit;

namespace Pixeval.Persisting
{
    public class Authentication
    {
        private const string ClientHash = "28c1fdd170a5204386cb1313c7077b34f83e4aaf4aa829ce78c231e05b0bae2c";

        private static string UtcTimeNow => DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss+00:00");

        /// <summary>
        ///     Authorize
        /// </summary>
        /// <param name="name">user account</param>
        /// <param name="pwd">user password</param>
        /// <returns>the <see cref="Task" />> of the auth process</returns>
        public static async Task Authenticate(string name, string pwd)
        {
            var time = UtcTimeNow;
            var hash = Cipher.Md5Hex(time + ClientHash);

            try
            {
                var token = await RestService.For<ITokenProtocol>(HttpClientFactory.PixivApi(ProtocolBase.OAuthBaseUrl, h => h.Timeout = TimeSpan.FromSeconds(10)))
                    .GetToken(new TokenRequest {Name = name, Password = pwd}, time, hash);

                Identity.Global = Identity.Parse(pwd, token);
            }
            catch (TaskCanceledException)
            {
                throw new TokenNotFoundException("请求超时, 请仔细检查您的网络环境");
            }
        }
    }
}