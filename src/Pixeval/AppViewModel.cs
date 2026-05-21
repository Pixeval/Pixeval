// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Imouto.BooruParser;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.AppManagement;
using Pixeval.Caching;
using Mako;
using Mako.Net;
using Mako.Model;
using Misaki;
using Pixeval.Database.Managers;
using Pixeval.Download;
using Pixeval.Extensions;
using Pixeval.Util.IO.Caching;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval;

public partial class AppViewModel(App app) : IDisposable
{
    public ServiceProvider AppServiceProvider { get; private set; } = null!;

    public IServiceScope AppServicesScope => AppServiceProvider.CreateScope();

    public App App { get; } = app;

    public DownloadManager DownloadManager { get; private set; } = null!;

    public MakoClient MakoClient => (MakoClient) GetRequiredPlatformService<IGetArtworkService>(IPlatformInfo.Pixiv);

    public AppSettings AppSettings { get; } = AppInfo.LoadConfig() ?? new AppSettings();

    public LoginContext LoginContext { get; } = AppInfo.LoadLoginContext() ?? new LoginContext();

    public AppDebugTrace AppDebugTrace { get; } = AppInfo.LoadDebugTrace() ?? new AppDebugTrace();

    public VersionContext VersionContext { get; } = AppInfo.LoadVersionContext() ?? new VersionContext();

    public long PixivUid => MakoClient.Me!.Id;

    public void AppLoggedIn()
    {
        DownloadManager = new DownloadManager(MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi), AppSettings.MaxDownloadTaskConcurrencyLevel);
        AppInfo.RestoreHistories();
    }

    private static ServiceProvider CreateServiceProvider()
    {
        const int defaultCacheSizeInByte = 200 * 1024 * 1024;

        var cacheTable = new CacheTable<PixevalIllustrationCacheKey, PixevalIllustrationCacheHeader, PixevalIllustrationCacheProtocol>(
            new PixevalIllustrationCacheProtocol(),
            new CacheToken(1, defaultCacheSizeInByte, AppKnownFolders.Cache.CombinePath(CacheHelper.CacheFolderName), 8));
        var fileLogger = new FileLogger(AppKnownFolders.Logs.FullPath);
        var extensionService = new ExtensionService();
        extensionService.LoadAllHosts(fileLogger);
        var makoClient = new MakoClient(App.AppViewModel.AppSettings.ToMakoConfiguration(), fileLogger);
        makoClient.TokenRefreshed += MakoClientOnTokenRefreshed;

        return new ServiceCollection()
            .AddSingleton(_ => fileLogger)
            .AddBooruParsers()
            .AddKeyedSingleton<IGetArtworkService, MakoClient>(IPlatformInfo.Pixiv,
                (provider, key) => makoClient)
            .AddKeyedSingleton<IDownloadHttpClientService, MakoClient>(IPlatformInfo.Pixiv,
                (provider, key) => (MakoClient) provider.GetRequiredKeyedService<IGetArtworkService>(IPlatformInfo.Pixiv))
            .AddKeyedSingleton<IDownloadHttpClientService, MakoClient>(IPlatformInfo.Pixiv,
                (provider, key) => (MakoClient) provider.GetRequiredKeyedService<IGetArtworkService>(IPlatformInfo.Pixiv))
            .AddSingleton<IllustrationDownloadTaskFactory>()
            .AddSingleton<NovelDownloadTaskFactory>()
            .AddSingleton(extensionService)
            .AddSingleton(cacheTable)
            .AddSingleton(new LiteDatabase(AppInfo.DatabaseFilePath))
            .AddSingleton(provider => new DownloadHistoryPersistentManager(provider.GetRequiredService<LiteDatabase>(),
                App.AppViewModel.AppSettings.MaximumDownloadHistoryRecords))
            .AddSingleton(provider => new SearchHistoryPersistentManager(provider.GetRequiredService<LiteDatabase>(),
                App.AppViewModel.AppSettings.MaximumSearchHistoryRecords))
            .AddSingleton(provider => new BrowseHistoryPersistentManager(provider.GetRequiredService<LiteDatabase>(),
                App.AppViewModel.AppSettings.MaximumBrowseHistoryRecords))
            .BuildServiceProvider();

        static void MakoClientOnTokenRefreshed(MakoClient sender, TokenResponse? tokenResponse)
        {
            if (tokenResponse is null)
                App.AppViewModel.LoginContext.RefreshToken = "";
            else
            {
                App.AppViewModel.LoginContext.RefreshToken = tokenResponse.RefreshToken;
                App.AppViewModel.LoginContext.IsPremium = tokenResponse.User.IsPremium;
            }
        }
    }

    public T? GetPlatformService<T>(string platformKey) where T : IMisakiService
    {
        return AppServiceProvider.GetKeyedService<T>(platformKey)
               ?? AppServiceProvider.GetKeyedService<T>(IPlatformInfo.All);
    }

    public T GetRequiredPlatformService<T>(string platformKey) where T : IMisakiService
    {
        return AppServiceProvider.GetKeyedService<T>(platformKey)
               ?? AppServiceProvider.GetKeyedService<T>(IPlatformInfo.All)
               ?? ThrowHelper.NotSupported<T>($"No service found for {platformKey}");
    }

    public void Initialize()
    {
        AppKnownFolders.Temp.Clear();
        AppServiceProvider = CreateServiceProvider();
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        AppServiceProvider?.GetService<LiteDatabase>()?.Dispose();
        AppServiceProvider?.GetService<ExtensionService>()?.Dispose();
        AppServiceProvider?.Dispose();
        DownloadManager?.Dispose();
        MakoClient?.Dispose();
        GC.SuppressFinalize(this);
    }

    private bool _disposed;
}
