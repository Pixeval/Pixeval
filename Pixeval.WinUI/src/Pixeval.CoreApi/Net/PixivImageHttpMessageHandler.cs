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
            MakoHttpOptions.UseHttpScheme(request);
            var requestUri = request.RequestUri!;
            if (requestUri.Host == MakoHttpOptions.ImageHost)
            {
                if (MakoClient.Configuration.MirrorHost is { } mirror && mirror.IsNotNullOrBlank())
                {
                    request.RequestUri = mirror switch
                    {
                        _ when Uri.CheckHostName(mirror) is not UriHostNameType.Unknown => new UriBuilder(requestUri) {Host = mirror}.Uri,
                        _ when Uri.IsWellFormedUriString(mirror, UriKind.Absolute)      => new Uri(mirror).Let(mirrorUri => new UriBuilder(requestUri) {Host = mirrorUri!.Host, Scheme = mirrorUri.Scheme})!.Uri,
                        _                                                               => throw new UriFormatException("Expecting a valid Host or URI")
                    };
                }
            }

            INameResolver resolver = MakoClient.Configuration.Bypass
                ? MakoClient.Resolve<PixivImageNameResolver>()
                : MakoClient.Resolve<LocalMachineNameResolver>();
            return MakoHttpOptions.CreateHttpMessageInvoker(resolver).SendAsync(request, cancellationToken);
        }
    }
}