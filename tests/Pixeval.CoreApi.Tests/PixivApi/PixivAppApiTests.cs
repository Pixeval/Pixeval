using Pixeval.CoreApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pixeval.CoreApi.Enums;
using Pixeval.CoreApi.Net.Requests;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Pixeval.CoreApi.Tests.PixivApi;

public class PixivAppApiTests : TestBed<PixivApiServiceTestBedFixture>
{
    private readonly IPixivAuthService _authService;
    private readonly IPixivAppService _appService;
    private readonly SessionRefresher _sessionRefresher;
    public PixivAppApiTests(ITestOutputHelper testOutputHelper, PixivApiServiceTestBedFixture fixture) : base(testOutputHelper, fixture)
    {
        _sessionRefresher = _fixture.GetService<SessionRefresher>(testOutputHelper)!;
        _authService = _fixture.GetService<IPixivAuthService>(testOutputHelper)!;
        _appService = _fixture.GetService<IPixivAppService>(testOutputHelper)!;
    }

    [Fact]
    public async Task TestGetUserMeStateAsync()
    {
        var userMeState = await _appService.GetUserMeStateAsync();
        Assert.NotNull(userMeState);
        Assert.NotNull(userMeState.UserState);
    }

    [Theory]
    [InlineData(33239622)]
    public async Task TestGetUserDetailAsync(long id)
    {
        var userResponse = await _appService.GetUserDetailAsync(id);
        Assert.Equal(id, userResponse.UserEntity?.Id);
    }


    [Fact]
    public async Task TestGetRecommendedIllustrationsAsync()
    {
        var recommendations = await _appService.GetRecommendedIllustrationsAsync();
        Assert.NotNull(recommendations);
        Assert.NotNull(recommendations.Illustrations);
    }


    [Theory]
    [InlineData(98883012)]
    [InlineData(99263446)]
    [InlineData(101863410)]
    public async Task TestGetIllustrationDetailAsync(long illustrationId)
    {
        var illustrationResponse = await _appService.GetIllustrationDetailAsync(illustrationId);
        Assert.NotNull(illustrationResponse);
        Assert.NotNull(illustrationResponse.Illust);
        Assert.Equal(illustrationResponse.Illust.Id, illustrationId);
    }
}