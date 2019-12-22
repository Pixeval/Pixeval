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
using Pixeval.Data.Web.Protocol;
using Pixeval.Objects;
using Refit;

namespace Pixeval.Data.Web.Delegation
{
    public class HttpClientFactory
    {
        public static HttpClient AppApiHttpClient = PixivApi(ProtocolBase.AppApiBaseUrl).Apply(h => h.DefaultRequestHeaders.Add("Authorization", "Bearer"));
        
        public static IPublicApiProtocol PublicApiService { get; } = RestService.For<IPublicApiProtocol>(PixivApi(ProtocolBase.PublicApiBaseUrl));

        public static IAppApiProtocol AppApiService { get; } = RestService.For<IAppApiProtocol>(PixivApi(ProtocolBase.AppApiBaseUrl));

        public static HttpClient PixivApi(string baseAddress, Action<HttpClient> action = null)
        {
            return new HttpClient(PixivApiHttpClientHandler.Instance)
            {
                BaseAddress = new Uri(baseAddress)
            }.Apply(h => action?.Invoke(h));
        }

        public static HttpClient PixivImage(Action<HttpClient> action = null)
        {
            return new HttpClient(PixivImageHttpClientHandler.Instance)
                .Apply(h => action?.Invoke(h));
        }
    }
}