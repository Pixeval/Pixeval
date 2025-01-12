// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Pixeval.AppManagement;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Net;
using Pixeval.Database.Managers;
using Pixeval.Download;
using Pixeval.Extensions;
using Pixeval.Logging;
using Pixeval.Util.IO.Caching;
using Pixeval.Util.UI;
using WinUI3Utilities;

namespace Pixeval;

public partial class AppViewModel(App app) : IDisposable
{
    private bool _activatedByProtocol;

    public ServiceProvider AppServiceProvider { get; private set; } = null!;

    public IServiceScope AppServicesScope => AppServiceProvider.CreateScope();

    public App App { get; } = app;

    public DownloadManager DownloadManager { get; private set; } = null!;

    public MakoClient MakoClient { get; set; } = null!; // The null-state of MakoClient is transient

    public AppSettings AppSettings { get; } = AppInfo.LoadConfig() ?? new AppSettings();

    public LoginContext LoginContext { get; set; } = AppInfo.LoadLoginContext() ?? new LoginContext();

    public AppDebugTrace AppDebugTrace { get; set; } = AppInfo.LoadDebugTrace() ?? new AppDebugTrace();

    public FileCache Cache { get; private set; } = null!;

    public long PixivUid => MakoClient.Me.Id;

    public void AppLoggedIn()
    {
        DownloadManager = new DownloadManager(MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi), AppSettings.MaxDownloadTaskConcurrencyLevel);
        AppInfo.RestoreHistories();
    }

    private static async Task<ServiceProvider> CreateServiceProvider()
    {
        var fileCache = await FileCache.CreateDefaultAsync();
        var memoryCache = await MemoryCache.CreateDefaultAsync(200);
        var extensionService = new ExtensionService();
        extensionService.LoadAllHosts();
        return new ServiceCollection()
            .AddSingleton<IllustrationDownloadTaskFactory>()
            .AddSingleton<NovelDownloadTaskFactory>()
            .AddSingleton<ExtensionService>(extensionService)
            .AddSingleton<MemoryCache>(memoryCache)
            .AddSingleton<FileCache>(fileCache)
            .AddSingleton<FileLogger>(_ => new(AppKnownFolders.Logs.FullPath))
            .AddSingleton<LiteDatabase>(new LiteDatabase(AppInfo.DatabaseFilePath))
            .AddSingleton<DownloadHistoryPersistentManager>(provider => new(provider.GetRequiredService<LiteDatabase>(), App.AppViewModel.AppSettings.MaximumDownloadHistoryRecords))
            .AddSingleton<SearchHistoryPersistentManager>(provider => new(provider.GetRequiredService<LiteDatabase>(), App.AppViewModel.AppSettings.MaximumSearchHistoryRecords))
            .AddSingleton<BrowseHistoryPersistentManager>(provider => new(provider.GetRequiredService<LiteDatabase>(), App.AppViewModel.AppSettings.MaximumBrowseHistoryRecords))
            .BuildServiceProvider();
    }

    public async Task ShowExceptionDialogAsync(Exception e)
    {
        _ = await WindowFactory.RootWindow.Content.To<FrameworkElement>().CreateAcknowledgementAsync(MiscResources.ExceptionEncountered, e.ToString());
    }

    public async Task InitializeAsync(bool activatedByProtocol)
    {
        _activatedByProtocol = activatedByProtocol;

        AppKnownFolders.Temp.Clear();

        AppServiceProvider = await CreateServiceProvider();
    }

    /// <summary>
    /// Gets and resets the <see cref="_activatedByProtocol" /> field, used for one-time activation process
    /// during the app start
    /// </summary>
    public bool ConsumeProtocolActivation()
    {
        var original = _activatedByProtocol;
        _activatedByProtocol = false;
        return original;
    }

    public void Dispose()
    {
        AppServiceProvider?.GetRequiredService<LiteDatabase>().Dispose();
        AppServiceProvider?.GetRequiredService<ExtensionService>().Dispose();
        AppServiceProvider?.Dispose();
        DownloadManager?.Dispose();
        MakoClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}
