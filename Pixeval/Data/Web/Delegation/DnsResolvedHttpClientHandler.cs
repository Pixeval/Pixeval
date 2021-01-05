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
using System.Threading;
using System.Threading.Tasks;
using Pixeval.Core;
using Pixeval.Objects.Exceptions;
using Pixeval.Persisting;

namespace Pixeval.Data.Web.Delegation
{
    public abstract class DnsResolvedHttpClientHandler : HttpClientHandler
    {
        private readonly IHttpRequestInterceptor interceptor;
        private readonly ManualResetEvent refreshing = new ManualResetEvent(true);
        
        static DnsResolvedHttpClientHandler()
        {
            AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);
        }

        protected DnsResolvedHttpClientHandler(IHttpRequestInterceptor interceptor = null)
        {
            this.interceptor = interceptor;
            ServerCertificateCustomValidationCallback = DangerousAcceptAnyServerCertificateValidator;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!refreshing.WaitOne(TimeSpan.FromSeconds(10)))
            {
                throw new AuthenticateFailedException("timeout"); // TODO localization
            }

            if (Session.RefreshRequired())
            {
                refreshing.Reset();
                await PixivClient.Refresh();
                refreshing.Set();
            }
            
            interceptor?.Intercept(this, request);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}