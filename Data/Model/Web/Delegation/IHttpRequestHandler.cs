using System.Net.Http;

namespace Pixeval.Data.Model.Web.Delegation
{
    public interface IHttpRequestHandler
    {
        void Handle(HttpRequestMessage httpRequestMessage);
    }
}