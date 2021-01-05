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
using Refit;

namespace Pixeval.Data.Web.Delegation
{
    public class HttpClientFactory
    {
        public const string AppApiBaseUrl = "https://app-api.pixiv.net";

        public const string DnsServer = "https://1.0.0.1";

        public const string SauceNaoUrl = "https://saucenao.com/";

        public const string OAuthBaseUrl = "https://oauth.secure.pixiv.net";

        public const string WebApiBaseUrl = "https://www.pixiv.net";

        public static HttpClient AppApiHttpClient()
        {
            return PixivApi().Apply(h => h.DefaultRequestHeaders.Add("Authorization", "Bearer"));
        }

        public static HttpClient WebApiHttpClient()
        {
            return PixivWebApi();
        }

        public static IAppApiProtocol AppApiService()
        {
            return RestService.For<IAppApiProtocol>(PixivApi());
        }

        public static IWebApiProtocol WebApiService()
        {
            return RestService.For<IWebApiProtocol>(PixivWebApi());
        }

        public static HttpClient PixivApi()
        {
            return PixivApiHttpClient;
        }

        public static HttpClient PixivImage()
        {
            return PixivImageHttpClient;
        }

        public static HttpClient PixivWebApi()
        {
            return PixivWebApiHttpClient;
        }

        public static HttpClient PixivAuthApi()
        {
            return PixivAuthHttpClient;
        }

        // -------------------------- Could you please using dependency injection? --------------------------

        // from author of Pixeval: the following code is awful at no matter extensibility, maintainability or
        // readability, it's horrible because it's designed for using per instance instead of a application-
        // lifecycle, static object
        private static readonly HttpClient PixivApiHttpClient = new HttpClient(PixivApiHttpClientHandler.Instance()) { BaseAddress = new Uri(AppApiBaseUrl) };

        private static readonly HttpClient PixivWebApiHttpClient = new HttpClient(PixivApiHttpClientHandler.Instance()) { BaseAddress = new Uri(WebApiBaseUrl) };

        private static readonly HttpClient PixivAuthHttpClient = new HttpClient(PixivApiHttpClientHandler.Instance()) { BaseAddress = new Uri(OAuthBaseUrl) };

        private static readonly HttpClient PixivImageHttpClient = new HttpClient(PixivImageHttpClientHandler.Instance).Apply(client =>
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "http://www.pixiv.net");
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "PixivIOSApp/5.8.7");
        });

        // --------------------- Thank the god I don't have to watch this piece of shit ---------------------

        public static Task<HttpResponseMessage> GetResponseHeader(HttpClient client, string uri)
        {
            return client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
        }
    }
}