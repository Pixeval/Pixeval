// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Imouto.BooruParser;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.AppManagement;
using Pixeval.Caching;
using Mako;
using Mako.Engine;
using Mako.Net;
using Misaki;
using Pixeval.Database.Managers;
using Pixeval.Download;
using Pixeval.Extensions;
using Pixeval.Util.IO.Caching;
using Pixeval.Utilities;
using WinUI3Utilities;
using Mako.Model;

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

    public HttpClient HttpClient { get; } = new()
    {
        BaseAddress = new Uri("https://localhost:7259")
    };

    public async Task<T> GetFromJsonAsync<T>(string uri, long uid = -1, params IReadOnlyList<(string, string)> paramsList)
    {
        return (T) (await HttpClient.GetFromJsonAsync(uri + $"?userId={(uid is -1 ? PixivUid : uid)}" + string.Concat(paramsList.Select(p => $"&{p.Item1}={p.Item2}")), typeof(T), AppJsonSerializerContext.Default))!;
    }

    public async Task<IFetchEngine<T>> GetEngineAsync<T>(string uri, long uid = -1, params IReadOnlyList<(string, string)> paramsList)
    {
        try
        {
            return MakoClient.Computed((await GetFromJsonAsync<IReadOnlyList<T>>(uri, uid, paramsList)).ToAsyncEnumerable());
        }
        catch (Exception e)
        {
            return MakoClient.Computed(Array.Empty<T>().ToAsyncEnumerable());
        }
    }

    public long PixivUid => MakoClient.Me.Id;

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
        var extensionService = new ExtensionService();
        extensionService.LoadAllHosts();
        return new ServiceCollection()
            .AddSingleton(_ => new FileLogger(AppKnownFolders.Logs.FullPath))
            .AddBooruParsers()
            .AddKeyedSingleton<IGetArtworkService, MakoClient>(IPlatformInfo.Pixiv,
                (provider, key) => new(App.AppViewModel.AppSettings.ToMakoClientConfiguration(), provider.GetRequiredService<FileLogger>()))
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
