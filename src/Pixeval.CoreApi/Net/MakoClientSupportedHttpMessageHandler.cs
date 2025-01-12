// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Net.Http;
using Pixeval.CoreApi.Global;

namespace Pixeval.CoreApi.Net;

public abstract class MakoClientSupportedHttpMessageHandler(MakoClient makoClient) : HttpMessageHandler, IMakoClientSupport
{
    public MakoClient MakoClient { get; set; } = makoClient;

    public HttpMessageInvoker GetHttpMessageInvoker(bool domainFronting)
    {
        return domainFronting ? MakoHttpOptions.CreateHttpMessageInvoker() : MakoHttpOptions.CreateDirectHttpMessageInvoker(MakoClient);
    }
}
