using System.Net.Http;

namespace Pixeval.Data.Web.Delegation
{
    public interface IHttpRequestHandler
    {
        void Handle(HttpRequestMessage httpRequestMessage);
    }
}