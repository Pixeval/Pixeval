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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.Persisting;

namespace Pixeval.Data.Web.Delegation
{
    public abstract class DnsResolvedHttpClientHandler : HttpClientHandler
    {
        private readonly bool _directConnect;
        private readonly IHttpRequestHandler _requestHandler;

        static DnsResolvedHttpClientHandler()
        {
            System.AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);
        }

        protected DnsResolvedHttpClientHandler(IHttpRequestHandler requestHandler = null, bool directConnect = true)
        {
            this._requestHandler = requestHandler;
            this._directConnect = directConnect;
            ServerCertificateCustomValidationCallback = DangerousAcceptAnyServerCertificateValidator;
        }

        protected abstract DnsResolver DnsResolver { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                                     CancellationToken cancellationToken)
        {
            _requestHandler?.Handle(request);

            if (_directConnect)
            {
                var host = request.RequestUri.DnsSafeHost;

                var isSslSession = request.RequestUri.ToString().StartsWith("https://");

                request.RequestUri =
                    new Uri(
                        $"{(isSslSession ? "https://" : "http://")}{(await DnsResolver.Lookup(host))[0]}{request.RequestUri.PathAndQuery}");
                request.Headers.Host = host;
            }

            HttpResponseMessage result;
            try
            {
                result = await base.SendAsync(request, cancellationToken);
            }
            catch (HttpRequestException e)
            {
                if (e.InnerException != null && e.InnerException.Message.ToLower().Contains("winhttp")) return new HttpResponseMessage(HttpStatusCode.OK);
                throw;
            }

            if (result.StatusCode == HttpStatusCode.BadRequest &&
                (await result.Content.ReadAsStringAsync()).Contains("OAuth"))
            {
                using var semaphore = new SemaphoreSlim(1);
                await semaphore.WaitAsync(cancellationToken);
                await Authentication.AppApiAuthenticate(Session.Current.Account, Session.Current.Password);
                var token = request.Headers.Authorization;
                if (token != null)
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue(token.Scheme, Session.Current.AccessToken);

                return await base.SendAsync(request, cancellationToken);
            }

            return result;
        }
    }
}
