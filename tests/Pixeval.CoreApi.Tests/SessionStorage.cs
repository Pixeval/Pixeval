using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Pixeval.CoreApi.Tests
{
    internal class SessionStorage
    {
        private string _userId;
        private string _refreshToken;
        private string _accessToken;

        public SessionStorage(string refreshToken)
        {
            _refreshToken = refreshToken;
        }

        public Task<UserSession?> GetSessionAsync(string? userId = null)
        {
            return Task.FromResult(new UserSession(_userId, _refreshToken, _accessToken, DateTimeOffset.Now))!;
        }

        public Task SetSessionAsync(string userId, string refreshToken, string accessToken)
        {
            _userId = userId;
            _refreshToken = refreshToken;
            _accessToken = accessToken;
            return Task.CompletedTask;
        }

        public Task ClearSessionAsync(string userId)
        {
            throw new NotImplementedException();
        }
    }
}
