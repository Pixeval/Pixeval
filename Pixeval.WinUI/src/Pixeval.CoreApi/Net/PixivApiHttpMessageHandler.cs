using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.CoreApi.Net
{
    internal class PixivApiHttpMessageHandler : MakoClientSupportedHttpMessageHandler
    {
        public PixivApiHttpMessageHandler(MakoClient makoClient)
        {
            MakoClient = makoClient;
        }
        
        public sealed override MakoClient MakoClient { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            MakoHttpOptions.UseHttpScheme(request);
            var headers = request.Headers;
            var host = request.RequestUri!.Host; // the 'RequestUri' is guaranteed to be nonnull here, because the 'HttpClient' will set it to 'BaseAddress' if its null

            headers.TryAddWithoutValidation("Accept-Language", MakoClient.Configuration.CultureInfo.Name);

            var session = MakoClient.Session;

            switch (host)
            {
                case MakoHttpOptions.WebApiHost:
                    headers.TryAddWithoutValidation("Cookie", session.Cookie);
                    break;
                case MakoHttpOptions.AppApiHost:
                    headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
                    break;
            }

            INameResolver resolver = MakoHttpOptions.BypassRequiredHost.IsMatch(host) && MakoClient.Configuration.Bypass
                ? MakoClient.Resolve<PixivApiNameResolver>()
                : MakoClient.Resolve<LocalMachineNameResolver>();
            return await MakoHttpOptions.CreateHttpMessageInvoker(resolver)
                .SendAsync(request, cancellationToken);
        }
    }
}