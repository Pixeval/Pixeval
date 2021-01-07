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
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Pixeval.Objects.I18n;
using Pixeval.Objects.Primitive;
using Pixeval.Persisting;

namespace Pixeval.Data.Web.Delegation
{
    public class PixivHttpRequestHandler : IHttpRequestInterceptor
    {
        public static readonly PixivHttpRequestHandler Instance = new PixivHttpRequestHandler();

        protected PixivHttpRequestHandler()
        {
        }

        public virtual void Intercept(DnsResolvedHttpClientHandler dnsResolvedHttpClientHandler, HttpRequestMessage httpRequestMessage)
        {
            // Regex is rather slower than multiple case-and-when clauses, but it can significantly improve the readability
            const string ApiHost = "^app-api\\.pixiv\\.net$";
            const string WebApiHost = "^(pixiv\\.net)|(www\\.pixiv\\.net)$";
            const string OAuthHost = "^oauth\\.secure\\.pixiv\\.net$";
            const string BypassHost = ApiHost + "|" + WebApiHost;

            switch (httpRequestMessage.RequestUri.IdnHost)
            {
                case var x when Regex.IsMatch(x, ApiHost):
                    var token = httpRequestMessage.Headers.Authorization;
                    if (token == null)
                    {
                        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Session.Current.AccessToken);
                    }
                    break;
                
                case var x when Regex.IsMatch(x, BypassHost) && Settings.Global.DirectConnect || Regex.IsMatch(x, OAuthHost):
                    ReplaceApiRequest(httpRequestMessage);
                    if (Regex.IsMatch(x, WebApiHost))
                    {
                        httpRequestMessage.Headers.TryAddWithoutValidation("Cookie", Settings.Global.Cookie);
                    }
                    break;
                
                case "i.pximg.net" when !Settings.Global.MirrorServer.IsNullOrEmpty():
                    httpRequestMessage.RequestUri = new Uri(httpRequestMessage.RequestUri.ToString().Replace("https://i.pximg.net", Settings.Global.MirrorServer));
                    break;
            }
            if (!httpRequestMessage.Headers.Contains("Accept-Language"))
            {
                httpRequestMessage.Headers.TryAddWithoutValidation("Accept-Language", AkaI18N.GetCultureAcceptLanguage());
            }
        }

        private static void ReplaceApiRequest(HttpRequestMessage request)
        {
            var host = request.RequestUri.IdnHost;

            var isSslSession = request.RequestUri.ToString().StartsWith("https://");

            request.RequestUri = new Uri($"{(isSslSession ? "https://" : "http://")}{PixivApiDnsResolver.Instance.Lookup().First()}{request.RequestUri.PathAndQuery}");
            request.Headers.Host = host;
        }
    }
}