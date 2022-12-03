using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Models;
using Pixeval.CoreApi.Services;
using WinUIEx;

namespace Pixeval
{
    internal class SessionRefresher:ISessionRefresher
    {
        private readonly IPixivAuthService _authService;

        public SessionRefresher(IPixivAuthService authService)
        {
            _authService = authService;
        }

        public async Task<TokenResponse> ExchangeTokenAsync()
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            var response = await loginWindow.LoginTask;
            loginWindow.Close();
            return response;
        }

        public Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            return _authService.RefreshAsync(refreshToken);
        }
    }
}
