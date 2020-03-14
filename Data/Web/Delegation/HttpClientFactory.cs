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
using System.Net.Http;
using System.Threading.Tasks;
using Pixeval.Data.Web.Protocol;
using Pixeval.Objects;
using Pixeval.Persisting;
using Refit;

namespace Pixeval.Data.Web.Delegation
{
    public class HttpClientFactory
    {
        public static HttpClient AppApiHttpClient()
        {
            return PixivApi(ProtocolBase.AppApiBaseUrl, Settings.Global.DirectConnect).Apply(h => h.DefaultRequestHeaders.Add("Authorization", "Bearer"));
        }

        public static IAppApiProtocol AppApiService()
        {
            return RestService.For<IAppApiProtocol>(PixivApi(ProtocolBase.AppApiBaseUrl, Settings.Global.DirectConnect));
        }

        public static HttpClient PixivApi(string baseAddress, bool directConnect)
        {
            return new HttpClient(PixivApiHttpClientHandler.Instance(directConnect))
            {
                BaseAddress = new Uri(baseAddress)
            };
        }

        public static HttpClient PixivImage()
        {
            return new HttpClient(PixivImageHttpClientHandler.Instance)
                .Apply(client =>
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "http://www.pixiv.net");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "PixivIOSApp/5.8.7");
                });
        }

        public static Task<HttpResponseMessage> GetResponseHeader(HttpClient client, string uri)
        {
            return client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
        }
    }
}