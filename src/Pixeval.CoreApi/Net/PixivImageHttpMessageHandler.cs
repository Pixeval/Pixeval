#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/PixivImageHttpMessageHandler.cs
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Net
{
    internal class PixivImageHttpMessageHandler : MakoClientSupportedHttpMessageHandler
    {
        public PixivImageHttpMessageHandler(MakoClient makoClient)
        {
            MakoClient = makoClient;
        }

        public sealed override MakoClient MakoClient { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (MakoClient.Configuration.Bypass)
            {
                MakoHttpOptions.UseHttpScheme(request);
            }
            var requestUri = request.RequestUri!;
            if (requestUri.Host == MakoHttpOptions.ImageHost && MakoClient.Configuration.MirrorHost is { } mirror && mirror.IsNotNullOrBlank())
            {
                request.RequestUri = mirror switch
                {
                    _ when Uri.CheckHostName(mirror) is not UriHostNameType.Unknown => new UriBuilder(requestUri) {Host = mirror}.Uri,
                    _ when Uri.IsWellFormedUriString(mirror, UriKind.Absolute)      => new Uri(mirror).Let(mirrorUri => new UriBuilder(requestUri) {Host = mirrorUri!.Host, Scheme = mirrorUri.Scheme})!.Uri,
                    _                                                               => throw new UriFormatException("Expecting a valid Host or URI")
                };
            }

            return MakoClient.GetHttpMessageInvoker(
                MakoClient.Configuration.Bypass ? typeof(PixivImageNameResolver) : typeof(LocalMachineNameResolver)
            ).SendAsync(request, cancellationToken);
        }
    }
}