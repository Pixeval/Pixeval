// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Imouto.BooruParser;
using Mako;
using Mako.Engine;
using Mako.Model;
using Mako.Net;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Models.Database;
using Pixeval.Models.Database.Managers;
using Pixeval.Models.Download;
using Pixeval.Models.Extensions;
using Pixeval.Models.McpServer;
using Pixeval.Models.Subscriptions;
using Pixeval.Utilities;
using Pixeval.Utilities.GitHub;
using Pixeval.Utilities.IO.Caching;
using Pixeval.Models.Options;
using Pixeval.Views;
using SQLite;

namespace Pixeval.AppManagement;

public sealed class AppViewModel(App app, FileLogger logger) : IAsyncDisposable
{
    private bool _disposed;

    public static bool UseMcpService { get; set; }

    public ServiceProvider AppServiceProvider { get; private set; } = null!;

    public App App { get; } = app;

    public HistoryPersistHelper HistoryPersistHelper => AppServiceProvider.GetRequiredService<HistoryPersistHelper>();

    public MakoClient MakoClient => (MakoClient) GetRequiredPlatformService<IGetArtworkService>(IPlatformInfo.Pixiv);

    public AppSettings AppSettings { get; } = AppInfo.LoadAppSettings(logger) ?? new AppSettings();

    public LoginContext LoginContext { get; } = AppInfo.LoadLoginContext(logger) ?? new LoginContext();

    public ObservableCollection<HomePageCardLayout> HomePageCards { get; } =
        AppInfo.LoadHomePageCards(logger) ?? HomePageCardsSettings.CreateDefaultCards();

    public void ResetHomePageCards()
    {
        HomePageCards.Clear();
        foreach (var card in HomePageCardsSettings.CreateDefaultCards())
            HomePageCards.Add(card);
    }

    public void InitializeProvider()
    {
        AppServiceProvider = CreateServiceProvider();
        SetNameResolvers();
        // 触发卸载插件
        _ = AppServiceProvider.GetRequiredService<ExtensionService>();
        CacheHelper.EnforceCacheSizeLimit();
    }

    private ServiceProvider CreateServiceProvider()
    {
        var makoClient = new MakoClient(App.AppViewModel.AppSettings.ToMakoConfiguration(), logger);
        makoClient.TokenRefreshed += MakoClientOnTokenRefreshed;

        var services = new ServiceCollection()
            .AddSingleton(_ => logger)
            .AddBooruParsers()
            .AddKeyedSingleton<IGetArtworkService, MakoClient>(IPlatformInfo.Pixiv, (provider, key) => makoClient)
            .AddKeyedSingleton<IDownloadHttpClientService, MakoClient>(IPlatformInfo.Pixiv,
                (provider, key) => makoClient)
            .AddKeyedSingleton<IDownloadHttpClientService>(
                GitHubHttpClientProvider.PlatformKey,
                (_, _) => new GitHubHttpClientProvider(AppSettings.NetworkSettings))
            .AddSingleton<WorkSubscriptionDownloadService>()
            .AddSingleton<IllustrationDownloadTaskFactory>()
            .AddSingleton<NovelDownloadTaskFactory>()
            .AddSingleton(provider => new ExtensionService(provider.GetRequiredService<FileLogger>(), AppSettings))
            .AddSingleton(_ => new SQLiteConnection(AppInfo.DatabaseFilePath,
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex))
            .AddSingleton<DownloadHistoryPersistentManager>()
            .AddSingleton<WorkSubscriptionPersistentManager>()
            .AddSingleton<SearchHistoryPersistentManager>()
            .AddSingleton<BrowseHistoryPersistentManager>()
            .AddSingleton<WatchLaterPersistentManager>()
            .AddSingleton<LoginUserPersistentManager>()
            .AddSingleton<HistoryPersistHelper>();
        if (UseMcpService)
            services.AddSingleton<IPixevalMcpService>(t =>
                new PixevalMcpService(this, t.GetRequiredService<FileLogger>()));
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        void MakoClientOnTokenRefreshed(MakoClient sender, TokenResponse? tokenResponse)
        {
            if (tokenResponse is null)
                LoginContext.CurrentKey = 0;
            else
            {
                var manager = AppServiceProvider.GetRequiredService<LoginUserPersistentManager>();
                var entry = manager.Upsert(LoginUserEntry.FromTokenUser(tokenResponse.RefreshToken,
                    tokenResponse.User));
                LoginContext.CurrentKey = entry.HistoryEntryId;
            }

            PixevalSettings.Instance.OnIsLoggedInChanged();
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

    public void QueueWorkSubscriptionSyncCurrentSource(long uid, WorkSubscriptionType subscriptionType,
        WorkSubscriptionWorkKind workKind, IFetchEngine<IWorkEntry> engine)
    {
        AppServiceProvider.GetRequiredService<WorkSubscriptionDownloadService>()
            .QueueSyncCurrentSource(uid, subscriptionType, workKind, engine);
    }

    public void SetNameResolvers()
    {
        var networkSettings = AppSettings.NetworkSettings;
        SetNameResolver(MakoHttpOptions.AppApiHost, networkSettings.PixivAppApiNameResolver);
        networkSettings.PixivAppApiNameResolver.CollectionChanged += (sender, e) =>
        {
            if (sender is IEnumerable<string> ips)
                SetNameResolver(MakoHttpOptions.AppApiHost, ips);
        };
        SetNameResolver(MakoHttpOptions.ImageHost, networkSettings.PixivImageNameResolver);
        networkSettings.PixivImageNameResolver.CollectionChanged += (sender, e) =>
        {
            if (sender is IEnumerable<string> ips)
                SetNameResolver(MakoHttpOptions.ImageHost, ips);
        };
        SetNameResolver(MakoHttpOptions.ImageHost2, networkSettings.PixivImageNameResolver2);
        networkSettings.PixivImageNameResolver2.CollectionChanged += (sender, e) =>
        {
            if (sender is IEnumerable<string> ips)
                SetNameResolver(MakoHttpOptions.ImageHost2, ips);
        };
        SetNameResolver(MakoHttpOptions.OAuthHost, networkSettings.PixivOAuthNameResolver);
        networkSettings.PixivOAuthNameResolver.CollectionChanged += (sender, e) =>
        {
            if (sender is IEnumerable<string> ips)
                SetNameResolver(MakoHttpOptions.OAuthHost, ips);
        };
        SetNameResolver(MakoHttpOptions.AccountHost, networkSettings.PixivAccountNameResolver);
        networkSettings.PixivAccountNameResolver.CollectionChanged += (sender, e) =>
        {
            if (sender is IEnumerable<string> ips)
                SetNameResolver(MakoHttpOptions.AccountHost, ips);
        };
        SetNameResolver(MakoHttpOptions.WebApiHost, networkSettings.PixivWebApiNameResolver);
        networkSettings.PixivWebApiNameResolver.CollectionChanged += (sender, e) =>
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

    public HttpClient GetRequiredHttpClient()
    {
        // 在 AddBooruParsers 中注册的
        return AppServiceProvider.GetRequiredKeyedService<IDownloadHttpClientService>(IPlatformInfo.All)
            .GetImageDownloadClient();
    }

    public HttpClient GetRequiredGitHubHttpClient() =>
        AppServiceProvider.GetRequiredKeyedService<IDownloadHttpClientService>(GitHubHttpClientProvider.PlatformKey)
            .GetApiClient();

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _disposed = true;
        try
        {
            // 有些服务只能 DisposeAsync
            await AppServiceProvider.DisposeAsync();
        }
        catch
        {
            // ignored
            // 保证退出时不出幺蛾子
        }
    }
}
