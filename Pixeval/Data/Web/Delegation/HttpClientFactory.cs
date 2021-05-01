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
using System.Threading.Tasks;
using Pixeval.Data.Web.Protocol;
using Pixeval.Objects.Generic;
using Pixeval.Persisting;
using Refit;

// ReSharper disable MemberCanBePrivate.Global
namespace Pixeval.Data.Web.Delegation
{
    public static class HttpClientFactory
    {
        public const string AppApiBaseUrl = "https://app-api.pixiv.net";
        public const string DnsServer = "https://1.0.0.1";
        public const string SauceNaoUrl = "https://saucenao.com/";
        public const string OAuthBaseUrl = "https://oauth.secure.pixiv.net";
        public const string WebApiBaseUrl = "https://www.pixiv.net";

        public static readonly HttpMessageHandler PixivHandler = PixivApiHttpClientHandler.Instance(Settings.Global.DirectConnect);

        public static HttpClient AppApi = new HttpClient(PixivHandler) { BaseAddress = new Uri(AppApiBaseUrl) };
        public static HttpClient WebApi = new HttpClient(PixivHandler)
        {
            BaseAddress = new Uri(WebApiBaseUrl)
        }.Apply(h => h.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.72 Safari/537.36 Edg/89.0.774.45"));
        public static readonly HttpClient Image = new HttpClient(PixivImageHttpClientHandler.Instance).Apply(client =>
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "http://www.pixiv.net");
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "PixivIOSApp/5.8.7");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        public static readonly IAppApiProtocol AppApiService = RestService.For<IAppApiProtocol>(AppApi);
        public static readonly IWebApiProtocol WebApiService = RestService.For<IWebApiProtocol>(WebApi);

        public static Task<HttpResponseMessage> GetResponseHeader(this HttpClient client, string uri)
        {
            return client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
        }
    }
}