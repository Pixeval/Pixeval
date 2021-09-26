#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/RetryHttpClientHandler.cs
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
using Pixeval.CoreApi.Global;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Net
{
    internal class RetryHttpClientHandler : HttpMessageHandler
    {
        private readonly int _timeout;
        private readonly HttpMessageInvoker _delegatedHandler;

        public RetryHttpClientHandler(HttpMessageHandler delegatedHandler, int timeout)
        {
            _timeout = timeout;
            _delegatedHandler = new HttpMessageInvoker(delegatedHandler);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await Functions.RetryAsync(() => _delegatedHandler.SendAsync(request, cancellationToken), 2, _timeout).ConfigureAwait(false) switch
            {
                Result<HttpResponseMessage>.Success(var response) => response,
                Result<HttpResponseMessage>.Failure failure       => throw failure.Cause ?? new HttpRequestException(),
                _                                                 => throw new InvalidOperationException("Unexpected case")
            };
        }
    }

    internal class MakoRetryHttpClientHandler : HttpMessageHandler, IMakoClientSupport
    {
        private readonly HttpMessageInvoker _delegatedHandler;

        public MakoRetryHttpClientHandler(HttpMessageHandler delegatedHandler)
        {
            _delegatedHandler = new HttpMessageInvoker(delegatedHandler);
        }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global ! Dependency Injected
        public MakoClient MakoClient { get; set; } = null!;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await Functions.RetryAsync(() => _delegatedHandler.SendAsync(request, cancellationToken), 2, MakoClient!.Configuration.ConnectionTimeout).ConfigureAwait(false) switch
            {
                Result<HttpResponseMessage>.Success (var response) => response,
                Result<HttpResponseMessage>.Failure failure => throw failure.Cause ?? new HttpRequestException(),
                _ => throw new InvalidOperationException("Unexpected case")
            };
        }
    }
}