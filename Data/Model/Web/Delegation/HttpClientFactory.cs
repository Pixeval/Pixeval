using System;
using System.Net.Http;
using Pixeval.Data.Model.Web.Protocol;
using Refit;

namespace Pixeval.Data.Model.Web.Delegation
{
    public class HttpClientFactory
    {
        public static HttpClient PixivApi(string baseAddress) => new HttpClient(PixivApiHttpClientHandler.Instance)
        {
            BaseAddress = new Uri(baseAddress)
        };

        public static HttpClient PixivImage() => new HttpClient(PixivImageHttpClientHandler.Instance);

        public static readonly IPublicApiProtocol PublicApiService = RestService.For<IPublicApiProtocol>(PixivApi(ProtocolBase.PublicApiBaseUrl));

        public static readonly IAppApiProtocol AppApiService = RestService.For<IAppApiProtocol>(PixivApi(ProtocolBase.AppApiBaseUrl));
    }
}