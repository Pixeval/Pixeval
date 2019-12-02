using System.Net.Http;

namespace Pzxlane.Data.Model.Web.Delegation
{
    public interface IHttpRequestHandler
    {
        void Handle(HttpRequestMessage httpRequestMessage);
    }
}