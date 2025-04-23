// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.AppManagement;
using Pixeval.Caching;
using Mako;
using Mako.Net;
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

    public MakoClient MakoClient { get; set; } = null!; // The null-state of MakoClient is transient

    public AppSettings AppSettings { get; } = AppInfo.LoadConfig() ?? new AppSettings();

    public LoginContext LoginContext { get; } = AppInfo.LoadLoginContext() ?? new LoginContext();

    public AppDebugTrace AppDebugTrace { get; } = AppInfo.LoadDebugTrace() ?? new AppDebugTrace();

    public VersionContext VersionContext { get; } = AppInfo.LoadVersionContext() ?? new VersionContext();

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
            .AddSingleton<IllustrationDownloadTaskFactory>()
            .AddSingleton<NovelDownloadTaskFactory>()
            .AddSingleton(extensionService)
            .AddSingleton(cacheTable)
            .AddSingleton(_ => new FileLogger(AppKnownFolders.Logs.FullPath))
            .AddSingleton(new LiteDatabase(AppInfo.DatabaseFilePath))
            .AddSingleton(provider => new DownloadHistoryPersistentManager(provider.GetRequiredService<LiteDatabase>(), App.AppViewModel.AppSettings.MaximumDownloadHistoryRecords))
            .AddSingleton(provider => new SearchHistoryPersistentManager(provider.GetRequiredService<LiteDatabase>(), App.AppViewModel.AppSettings.MaximumSearchHistoryRecords))
            .AddSingleton(provider => new BrowseHistoryPersistentManager(provider.GetRequiredService<LiteDatabase>(), App.AppViewModel.AppSettings.MaximumBrowseHistoryRecords))
            .BuildServiceProvider();
    }

    public IImageUriDownloadProvider GetDownloadProvider(string platformKey)
    {
        return App.AppViewModel.AppServiceProvider.GetKeyedService<IImageUriDownloadProvider>(platformKey)
               ?? App.AppViewModel.AppServiceProvider.GetService<IImageUriDownloadProvider>()
               ?? ThrowHelper.NotSupported<IImageUriDownloadProvider>($"No provider found for {platformKey}");
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
