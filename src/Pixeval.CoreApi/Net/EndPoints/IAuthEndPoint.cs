// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Threading.Tasks;
using Mako.Model;
using Mako.Net.Request;
using WebApiClientCore;
using WebApiClientCore.Attributes;

namespace Mako.Net.EndPoints;

[Header(HttpRequestHeader.UserAgent, "PixivAndroidApp/5.0.64 (Android 6.0)")]
[Header(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded")]
[HttpHost(MakoHttpOptions.OAuthBaseUrl)]
public interface IAuthEndPoint
{
    [HttpPost("/auth/token")]
    Task<TokenResponse> RefreshAsync([FormContent] RefreshSessionRequest request);
}
