#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/AppViewModel.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

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
using Pixeval.Settings;
using Pixeval.Util.IO.Caching;
using Pixeval.Util.UI;
using WinUI3Utilities;

namespace Pixeval;

public partial class AppViewModel : IDisposable
{
    private bool _activatedByProtocol;

    public AppViewModel(App app)
    {
        App = app;
        SettingsPair = new(AppSettings, AppInfo.LocalConfig);
    }

    public ServiceProvider AppServiceProvider { get; private set; } = null!;

    public IServiceScope AppServicesScope => AppServiceProvider.CreateScope();

    public App App { get; }

    public DownloadManager DownloadManager { get; private set; } = null!;

    public MakoClient MakoClient { get; set; } = null!; // The null-state of MakoClient is transient

    public AppSettings AppSettings { get; } = AppInfo.LoadConfig() ?? new AppSettings();

    public SettingsPair<AppSettings> SettingsPair { get; }

    public LoginContext LoginContext { get; set; } = AppInfo.LoadLoginContext() ?? new LoginContext();

    public AppDebugTrace AppDebugTrace { get; set; } = AppInfo.LoadDebugTrace() ?? new AppDebugTrace();

    public FileCache Cache { get; private set; } = null!;

    public long PixivUid => MakoClient.Session.Id;

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
        extensionService.LoadAllExtensions();
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
        AppServiceProvider?.Dispose();
        DownloadManager?.Dispose();
        MakoClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}
