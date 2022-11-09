using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pixeval.CoreApi.Models;
using Pixeval.CoreApi.Net.Requests;

namespace Pixeval.CoreApi.Services
{
    [Headers("User-Agent: PixivAndroidApp/5.0.64 (Android 6.0)", "Content-Type: application/x-www-form-urlencoded")]
    public interface IPixivAuthService
    {

        [Post("/auth/token")]
        internal Task<TokenResponse> RefreshAsync([Body(BodySerializationMethod.UrlEncoded)] RefreshTokenRequest request);

        Task<TokenResponse> RefreshAsync(string refreshToken) => RefreshAsync(new RefreshTokenRequest(refreshToken));
    }
}
