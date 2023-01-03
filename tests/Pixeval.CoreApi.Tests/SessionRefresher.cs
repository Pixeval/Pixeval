using Newtonsoft.Json.Linq;
using Pixeval.CoreApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Pixeval.CoreApi.Models;

namespace Pixeval.CoreApi.Tests;

internal class SessionRefresher : ISessionRefresher
{
    private readonly IPixivAuthService _authService;
    private readonly IOptions<ApiOptions> _options;
    public SessionRefresher(IPixivAuthService authService, IOptions<ApiOptions> options)
    {
        _authService = authService;
        _options = options;
    }

    public async Task<string> GetAccessTokenAsync(string? refreshToken = null)
    {
        var response = await _authService.RefreshAsync(_options.Value.RefreshToken);
        return response.AccessToken;
    }
}