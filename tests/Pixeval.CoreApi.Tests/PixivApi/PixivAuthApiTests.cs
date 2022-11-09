using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Pixeval.CoreApi.Net.Requests;
using Pixeval.CoreApi.Services;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Pixeval.CoreApi.Tests.PixivApi
{
    public class PixivAuthApiTests : TestBed<PixivApiServiceTestBedFixture>
    {
        private readonly IPixivAuthService _authService;
        public PixivAuthApiTests(ITestOutputHelper testOutputHelper, PixivApiServiceTestBedFixture fixture) : base(testOutputHelper, fixture)
        {
            _authService = _fixture.GetService<IPixivAuthService>(_testOutputHelper)!;
        }

        [Fact]
        public async Task TestRefreshTokenAsync()
        {
            var tokenResponse = await _authService.RefreshAsync(_fixture.Configuration["RefreshToken"]!);
            Assert.NotNull(tokenResponse);
            Assert.False(string.IsNullOrWhiteSpace(tokenResponse.RefreshToken));
            Assert.False(string.IsNullOrWhiteSpace(tokenResponse.AccessToken));
        }
    }
}
