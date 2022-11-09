using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pixeval.CoreApi.Models;

namespace Pixeval.CoreApi
{
    public interface ISessionRefresher
    {
        Task<TokenResponse> ExchangeTokenAsync();

        Task<TokenResponse> RefreshTokenAsync(string refreshToken);
    }
}
