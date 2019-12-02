using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using Pzxlane.Caching.Persisting;
using Pzxlane.Objects;
using Pzxlane.Objects.Exceptions;

namespace Pzxlane.Data.Model.Web.Delegation
{
    public class PixivAuthenticationHttpRequestHandler : IHttpRequestHandler
    {
        private PixivAuthenticationHttpRequestHandler()
        {

        }

        public void Handle(HttpRequestMessage httpRequestMessage)
        {
            var token = httpRequestMessage.Headers.Authorization;
            if (token != null)
            {
                if (Identity.Global.AccessToken.IsNullOrEmpty())
                {
                    throw new TokenNotFoundException($"{nameof(Identity.Global.AccessToken)} is empty, this exception should never be thrown, if you see this message, please send issue on github or contact me (decem0730@gmail.com)");
                }

                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(token.Scheme, Identity.Global.AccessToken);
            }
        }

        public static PixivAuthenticationHttpRequestHandler Instance = new PixivAuthenticationHttpRequestHandler();
    }
}