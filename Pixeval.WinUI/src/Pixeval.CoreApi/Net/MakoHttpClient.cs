using System;
using System.Net.Http;
using JetBrains.Annotations;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Net
{
    internal class MakoHttpClient : HttpClient
    {
        private MakoHttpClient([NotNull] HttpMessageHandler handler) : base(handler)
        {
        }

        public static MakoHttpClient Create(HttpMessageHandler handler,
            Action<MakoHttpClient>? action = null)
        {
            var mako = new MakoHttpClient(handler);
            action?.Let(ac => ac!(mako));
            return mako;
        }
    }
}