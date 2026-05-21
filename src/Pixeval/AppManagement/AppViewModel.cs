// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Imouto.BooruParser;
using Mako;
using Mako.Engine;
using Mako.Model;
using Mako.Net;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Caching;
using Pixeval.Models.Database;
using Pixeval.Models.Database.Managers;
using Pixeval.Models.Download;
using Pixeval.Models.Subscriptions;
using Pixeval.Models.Extensions;
using Pixeval.Utilities;
using Pixeval.Utilities.IO.Caching;
using SQLite;
using Pixeval.Models.Options;

namespace Pixeval.AppManagement;

public class AppViewModel(App app, FileLogger logger) : IDisposable
{
    public ServiceProvider AppServiceProvider { get; private set; } = null!;

    public App App { get; } = app;

    public HistoryPersistHelper HistoryPersistHelper => AppServiceProvider.GetRequiredService<HistoryPersistHelper>();

    public MakoClient MakoClient => (MakoClient) GetRequiredPlatformService<IGetArtworkService>(IPlatformInfo.Pixiv);

    public AppSettings AppSettings { get; } = AppInfo.LoadConfig(logger) ?? new AppSettings();

    public LoginContext LoginContext { get; } = AppInfo.LoadLoginContext(logger) ?? new LoginContext();

    public long PixivUid => MakoClient.Me!.Id;

    public void InitializeProvider()
    {
        AppServiceProvider = CreateServiceProvider();
        SetNameResolvers();
    }

    private ServiceProvider CreateServiceProvider()
    {
        const int defaultCacheSizeInByte = 200 * 1024 * 1024;

        var cacheTable = new CacheTable<PixevalIllustrationCacheKey, PixevalIllustrationCacheHeader, PixevalIllustrationCacheProtocol>(
            new PixevalIllustrationCacheProtocol(),
            new CacheToken(1, defaultCacheSizeInByte, AppInfo.CacheFolder, 8));
        var makoClient = new MakoClient(App.AppViewModel.AppSettings.ToMakoConfiguration(), logger);
        makoClient.TokenRefreshed += MakoClientOnTokenRefreshed;

        return new ServiceCollection()
            .AddSingleton(_ => logger)
            .AddBooruParsers()
            .AddKeyedSingleton<IGetArtworkService, MakoClient>(IPlatformInfo.Pixiv, (provider, key) => makoClient)
            .AddKeyedSingleton<IDownloadHttpClientService, MakoClient>(IPlatformInfo.Pixiv, (provider, key) => makoClient)
            .AddSingleton<WorkSubscriptionDownloadService>()
            .AddSingleton<IllustrationDownloadTaskFactory>()
            .AddSingleton<NovelDownloadTaskFactory>()
            .AddSingleton<ExtensionService>()
            .AddSingleton(cacheTable)
            .AddSingleton(_ => new SQLiteConnection(AppInfo.DatabaseFilePath,
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex))
            .AddSingleton<DownloadHistoryPersistentManager>()
            .AddSingleton<WorkSubscriptionPersistentManager>()
            .AddSingleton<SearchHistoryPersistentManager>()
            .AddSingleton<BrowseHistoryPersistentManager>()
            .AddSingleton<LoginUserPersistentManager>()
            .AddSingleton<HistoryPersistHelper>()
            .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        void MakoClientOnTokenRefreshed(MakoClient sender, TokenResponse? tokenResponse)
        {
            if (tokenResponse is null)
            {
                LoginContext.CurrentKey = 0;
            }
            else
            {
                var manager = AppServiceProvider.GetRequiredService<LoginUserPersistentManager>();
                var entry = manager.Upsert(LoginUserEntry.FromTokenUser(tokenResponse.RefreshToken, tokenResponse.User));
                LoginContext.CurrentKey = entry.HistoryEntryId;
            }

            AppInfo.SaveLoginContext(LoginContext);
        }
    }

    public LoginUserEntry? GetCurrentLoginUser()
    {
        return AppServiceProvider.GetRequiredService<LoginUserPersistentManager>()
            .GetByKey(LoginContext.CurrentKey);
    }

    public void QueueWorkSubscriptionSyncAll()
    {
        AppServiceProvider.GetRequiredService<WorkSubscriptionDownloadService>().QueueSyncAll();
    }

    public void QueueWorkSubscriptionSyncCurrentSource(long uid, WorkSubscriptionType subscriptionType, WorkSubscriptionWorkKind workKind, IFetchEngine<IWorkEntry> engine)
    {
        AppServiceProvider.GetRequiredService<WorkSubscriptionDownloadService>().QueueSyncCurrentSource(uid, subscriptionType, workKind, engine);
    }

    public void SetNameResolvers()
    {
        SetNameResolver(MakoHttpOptions.AppApiHost, AppSettings.PixivAppApiNameResolver);
        AppSettings.PixivAppApiNameResolver.CollectionChanged += (sender, e) =>
        {
            if (sender is IEnumerable<string> ips)
                SetNameResolver(MakoHttpOptions.AppApiHost, ips);
        };
        SetNameResolver(MakoHttpOptions.ImageHost, AppSettings.PixivImageNameResolver);
        AppSettings.PixivAppApiNameResolver.CollectionChanged += (sender, e) =>
        {
            if (sender is IEnumerable<string> ips)
                SetNameResolver(MakoHttpOptions.ImageHost, ips);
        };
        SetNameResolver(MakoHttpOptions.ImageHost2, AppSettings.PixivImageNameResolver2);
        AppSettings.PixivAppApiNameResolver.CollectionChanged += (sender, e) =>
        {
            if (sender is IEnumerable<string> ips)
                SetNameResolver(MakoHttpOptions.ImageHost2, ips);
        };
        SetNameResolver(MakoHttpOptions.OAuthHost, AppSettings.PixivOAuthNameResolver);
        AppSettings.PixivAppApiNameResolver.CollectionChanged += (sender, e) =>
        {
            if (sender is IEnumerable<string> ips)
                SetNameResolver(MakoHttpOptions.OAuthHost, ips);
        };
        SetNameResolver(MakoHttpOptions.AccountHost, AppSettings.PixivAccountNameResolver);
        AppSettings.PixivAppApiNameResolver.CollectionChanged += (sender, e) =>
        {
            if (sender is IEnumerable<string> ips)
                SetNameResolver(MakoHttpOptions.AccountHost, ips);
        };
        SetNameResolver(MakoHttpOptions.WebApiHost, AppSettings.PixivWebApiNameResolver);
        AppSettings.PixivAppApiNameResolver.CollectionChanged += (sender, e) =>
        {
            if (sender is IEnumerable<string> ips)
                SetNameResolver(MakoHttpOptions.WebApiHost, ips);
        };
    }

    public void SetNameResolver(string host, IEnumerable<string> ips)
    {
        MakoClient.Configuration.NameResolvers[host] = [.. ips.Select(IPAddress.Parse)];
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
               ?? throw new NotSupportedException($"No service found for {platformKey}");
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        AppServiceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }

    private bool _disposed;
}
