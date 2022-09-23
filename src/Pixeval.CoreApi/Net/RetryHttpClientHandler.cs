#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/RetryHttpClientHandler.cs
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
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.CoreApi.Global;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Net;

internal class RetryHttpClientHandler : HttpMessageHandler
{
    private readonly HttpMessageInvoker _delegatedHandler;
    private readonly int _timeout;

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
            Result<HttpResponseMessage>.Failure failure => throw failure.Cause ?? new HttpRequestException(),
            _ => throw new InvalidOperationException("Unexpected case")
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
            Result<HttpResponseMessage>.Success(var response) => response,
            Result<HttpResponseMessage>.Failure failure => throw failure.Cause ?? new HttpRequestException(),
            _ => throw new InvalidOperationException("Unexpected case")
        };
    }
}