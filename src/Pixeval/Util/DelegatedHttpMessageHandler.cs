// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.Util;

public partial class DelegatedHttpMessageHandler(HttpMessageInvoker @delegate) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return @delegate.SendAsync(request, cancellationToken);
    }
}
