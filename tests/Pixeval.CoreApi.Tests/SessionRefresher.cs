using Newtonsoft.Json.Linq;
using Pixeval.CoreApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pixeval.CoreApi.Models;

namespace Pixeval.CoreApi.Tests
{
    internal class SessionRefresher : ISessionRefresher
    {
        private readonly IPixivAuthService _authService;
        public SessionRefresher(IPixivAuthService authService)
        {
            _authService = authService;
        }

        public Task<TokenResponse> ExchangeTokenAsync()
        {
            throw new NotImplementedException();
        }

        public Task<TokenResponse> GetRefreshTokenAsync(string refreshToken)
        {
            return _authService.RefreshAsync(refreshToken);
        }

        public async Task<string> GetAccessTokenAsync(string? refreshToken = null)
        {
            if (refreshToken is null)
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }
            var response = await _authService.RefreshAsync(refreshToken);
            return response.AccessToken;
        }
    }
}
