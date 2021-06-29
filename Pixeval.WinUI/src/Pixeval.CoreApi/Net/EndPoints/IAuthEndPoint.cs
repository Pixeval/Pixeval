using System.Threading.Tasks;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net.Request;
using Refit;

namespace Pixeval.CoreApi.Net.EndPoints
{
    internal interface IAuthEndPoint
    {
        [Post("/auth/token")]
        Task<TokenResponse> Refresh([Body(BodySerializationMethod.UrlEncoded)] RefreshSessionRequest request);
    }
}