// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mako.Model;
using Mako.Net.EndPoints;
using Mako.Net.Request;

namespace Mako.Net;

public class PixivTokenProvider(IServiceProvider serviceProvider, TokenResponse firstTokenResponse)
{
    private readonly SemaphoreSlim _asyncRoot = new(1, 1);

    private DateTime _lastRefreshTime = DateTime.Now;

    public TokenUser Me => _tokenResponse.User;

    private TokenResponse _tokenResponse = firstTokenResponse;

    public async Task<TokenResponse?> GetTokenAsync()
    {
        await _asyncRoot.WaitAsync().ConfigureAwait(false);
        try
        {
            if (string.IsNullOrWhiteSpace(_tokenResponse.AccessToken)
                || DateTime.Now > _lastRefreshTime + TimeSpan.FromSeconds(_tokenResponse.ExpiresIn))
            {
                var token = await serviceProvider.GetRequiredService<IAuthEndPoint>()
                    .RefreshAsync(new RefreshSessionRequest(_tokenResponse.RefreshToken)).ConfigureAwait(false);
                _tokenResponse = token;
                _lastRefreshTime = DateTime.Now;
                serviceProvider.GetRequiredService<MakoClient>().OnTokenRefreshed(_tokenResponse.User);
            }

            return _tokenResponse;
        }
        catch (Exception e)
        {
            serviceProvider.GetRequiredService<MakoClient>().OnTokenRefreshedFailed(e);
            return null;
        }
        finally
        {
            _ = _asyncRoot.Release();
        }
    }
}
