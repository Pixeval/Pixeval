// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.ObjectModel;
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
using Pixeval.Models.Home;
#if PIXEVAL_MCP
using Pixeval.Models.McpServer;
#endif
using Pixeval.Models.Navigation;
using Pixeval.Models.Options;
using Pixeval.Models.Subscriptions;
using Pixeval.Utilities;
using Pixeval.Utilities.GitHub;
using Pixeval.Utilities.IO.Caching;
using Pixeval.Views;
using SQLite;

namespace Pixeval.AppManagement;

public sealed class AppViewModel(App app, FileLogger logger) : IAsyncDisposable
{
    private bool _disposed;

    public ServiceProvider AppServiceProvider { get; private set; } = null!;

    public App App { get; } = app;

    public HistoryPersistHelper HistoryPersistHelper => AppServiceProvider.GetRequiredService<HistoryPersistHelper>();

    public MakoClient MakoClient => (MakoClient) GetRequiredPlatformService<IGetArtworkService>(IPlatformInfo.Pixiv);

    public AppSettings AppSettings { get; } = AppInfo.LoadAppSettings(logger) ?? new AppSettings();

    public LoginContext LoginContext { get; } = AppInfo.LoadLoginContext(logger) ?? new LoginContext();

    public ObservableCollection<HomePageCardLayout> HomePageCards { get; } = AppInfo.LoadHomePageCards(logger) ?? HomePageCardsSettings.CreateDefaultCards();

    public string NavigationMenuYamlText { get; set; } =
        AppInfo.LoadNavigationMenuYaml(logger) ?? NavigationMenuYaml.DefaultYaml;

    public void ResetHomePageCards()
    {
        HomePageCards.Clear();
        foreach (var card in HomePageCardsSettings.CreateDefaultCards())
            HomePageCards.Add(card);
    }

    public void InitializeProvider()
    {
        AppSettings.Initialize();
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

        return new ServiceCollection()
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
            .AddSingleton<HistoryPersistHelper>()
#if PIXEVAL_MCP
            .AddSingleton<IPixevalMcpService>(t =>
                new PixevalMcpService(this, t.GetRequiredService<FileLogger>()))
#endif
            .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

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
        SetNameResolver(MakoHttpOptions.ImageHost, networkSettings.PixivImageNameResolver);
        SetNameResolver(MakoHttpOptions.ImageHost2, networkSettings.PixivImageNameResolver2);
        SetNameResolver(MakoHttpOptions.OAuthHost, networkSettings.PixivOAuthNameResolver);
        SetNameResolver(MakoHttpOptions.AccountHost, networkSettings.PixivAccountNameResolver);
        SetNameResolver(MakoHttpOptions.WebApiHost, networkSettings.PixivWebApiNameResolver);

        SetNameResolver(GitHubHttpOptions.Host, networkSettings.GitHubNameResolver);
        SetNameResolver(GitHubHttpOptions.ApiHost, networkSettings.GitHubApiNameResolver);
        SetNameResolver(GitHubHttpOptions.AvatarHost, networkSettings.GitHubAvatarNameResolver);
        SetNameResolver(GitHubHttpOptions.UserContentHost, networkSettings.GitHubUserContentNameResolver);
        SetNameResolver(GitHubHttpOptions.AssetsHost, networkSettings.GitHubAssetsNameResolver);
        SetNameResolver(GitHubHttpOptions.CodeloadHost, networkSettings.GitHubCodeloadNameResolver);
        return;

        static void SetNameResolver(string host, ObservableCollection<string> ips)
        {
            App.AppViewModel.MakoClient.Configuration.NameResolvers[host] = [.. ips.SelectNotNull(static ip => IPAddress.TryParse(ip, out var address) ? address : null)];
            ips.CollectionChanged += static (sender, e) =>
            {
                if (sender is ObservableCollection<string> ips)
                    SetNameResolver(MakoHttpOptions.WebApiHost, ips);
            };
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
