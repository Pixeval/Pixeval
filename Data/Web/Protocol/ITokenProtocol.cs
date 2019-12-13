using System.Threading.Tasks;
using Pixeval.Data.Web.Request;
using Pixeval.Data.Web.Response;
using Refit;

namespace Pixeval.Data.Web.Protocol
{
    [Headers("User-Agent: PixivAndroidApp/5.0.64 (Android 6.0)", "Content-Type: application/x-www-form-urlencoded")]
    public interface ITokenProtocol
    {
        [Post("/auth/token")]
        Task<TokenResponse> GetToken([Body(BodySerializationMethod.UrlEncoded)]
            TokenRequest body, [Header("X-Client-Time")] string clientTime, [Header("X-Client-Hash")] string clientHash);
    }
}