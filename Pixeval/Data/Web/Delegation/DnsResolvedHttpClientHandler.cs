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
using Pixeval.Objects.Exceptions;
using Pixeval.Objects.I18n;
using Pixeval.Persisting;

namespace Pixeval.Data.Web.Delegation
{
    public abstract class DnsResolvedHttpClientHandler : HttpClientHandler
    {
        private readonly bool directConnect;
        private readonly IHttpRequestHandler requestHandler;
        private readonly ManualResetEvent refreshing = new ManualResetEvent(true);

        static DnsResolvedHttpClientHandler()
        {
            System.AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);
        }

        protected DnsResolvedHttpClientHandler(IHttpRequestHandler requestHandler = null, bool directConnect = true)
        {
            this.requestHandler = requestHandler;
            this.directConnect = directConnect;
            ServerCertificateCustomValidationCallback = DangerousAcceptAnyServerCertificateValidator;
        }

        protected abstract DnsResolver DnsResolver { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!refreshing.WaitOne(TimeSpan.FromSeconds(10)))
            {
                throw new AuthenticateFailedException(AkaI18N.AppApiAuthenticateTimeout);
            }

            if (Session.RefreshRequired(Session.Current) && request.RequestUri.Host != "oauth.secure.pixiv.net")
            {
                using var semaphore = new SemaphoreSlim(1);
                await semaphore.WaitAsync(cancellationToken);
                refreshing.Reset();
                await Authentication.Refresh(Session.Current.RefreshToken);
                refreshing.Set();
            }

            requestHandler?.Handle(request);

            if (directConnect)
            {
                var host = request.RequestUri.Host;

                var isSslSession = request.RequestUri.ToString().StartsWith("https://");

                request.RequestUri = new Uri($"{(isSslSession ? "https://" : "http://")}{DnsResolver.Lookup()[0]}{request.RequestUri.PathAndQuery}");
                request.Headers.Host = host;
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}