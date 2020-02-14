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

using System.Net.Http;
using System.Net.Http.Headers;
using Pixeval.Objects;
using Pixeval.Objects.Exceptions;
using Pixeval.Persisting;

namespace Pixeval.Data.Web.Delegation
{
    public class PixivAuthenticationHttpRequestHandler : IHttpRequestHandler
    {
        public static readonly PixivAuthenticationHttpRequestHandler Instance = new PixivAuthenticationHttpRequestHandler();

        protected PixivAuthenticationHttpRequestHandler() { }

        public virtual void Handle(HttpRequestMessage httpRequestMessage)
        {
            var token = httpRequestMessage.Headers.Authorization;
            if (token != null)
            {
                if (Identity.Global.AccessToken.IsNullOrEmpty()) throw new TokenNotFoundException($"{nameof(Identity.Global.AccessToken)} is empty, this exception should never be thrown, if you see this message, please send issue on github or contact me (decem0730@gmail.com)");

                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(token.Scheme, Identity.Global.AccessToken);
            }
        }
    }
}