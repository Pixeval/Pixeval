// Pixeval
// Copyright (C) 2019 Dylech30th <decem0730@gmail.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Net.Http;
using Pixeval.Data.Web.Protocol;
using Refit;

namespace Pixeval.Data.Web.Delegation
{
    public class HttpClientFactory
    {
        public static IPublicApiProtocol PublicApiService { get; } = RestService.For<IPublicApiProtocol>(PixivApi(ProtocolBase.PublicApiBaseUrl));

        public static IAppApiProtocol AppApiService { get; } = RestService.For<IAppApiProtocol>(PixivApi(ProtocolBase.AppApiBaseUrl));

        public static HttpClient PixivApi(string baseAddress)
        {
            return new HttpClient(PixivApiHttpClientHandler.Instance)
            {
                BaseAddress = new Uri(baseAddress)
            };
        }

        public static HttpClient PixivImage()
        {
            return new HttpClient(PixivImageHttpClientHandler.Instance);
        }
    }
}