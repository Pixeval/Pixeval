using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Models;
using Pixeval.CoreApi.Services;

namespace Pixeval
{
    internal class SessionRefresher:ISessionRefresher
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

        public Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            return _authService.RefreshAsync(refreshToken);
        }
    }
}
