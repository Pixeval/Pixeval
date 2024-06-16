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
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Net;
using Pixeval.Database.Managers;
using Pixeval.Download;
using Pixeval.Download.Models;
using Pixeval.Logging;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Windows.Storage;

namespace Pixeval;

public partial class AppViewModel(App app) : IDisposable
{
    private bool _activatedByProtocol;

    public ServiceProvider AppServiceProvider { get; private set; } = null!;

    public IServiceScope AppServicesScope => AppServiceProvider.CreateScope();

    public App App { get; } = app;

    public DownloadManager<DownloadTaskBase> DownloadManager { get; private set; } = null!;

    public MakoClient MakoClient { get; set; } = null!; // The null-state of MakoClient is transient

    public AppSettings AppSettings { get; } = AppInfo.LoadConfig() ?? new AppSettings();

    public LoginContext LoginContext { get; set; } = AppInfo.LoadLoginContext() ?? new LoginContext();

    public AppDebugTrace AppDebugTrace { get; set; } = AppInfo.LoadDebugTrace() ?? new AppDebugTrace();

    public FileCache Cache { get; private set; } = null!;

    public long PixivUid => MakoClient.Session.Id;

    public void AppLoggedIn()
    {
        DownloadManager = new DownloadManager<DownloadTaskBase>(AppSettings.MaxDownloadTaskConcurrencyLevel, MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi));
        AppInfo.RestoreHistories();
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var fileLogger = new FileLogger(ApplicationData.Current.LocalFolder.Path + @"\Logs\");
        return new ServiceCollection()
            .AddSingleton<IDownloadTaskFactory<IllustrationItemViewModel, IllustrationDownloadTask>, IllustrationDownloadTaskFactory>()
            .AddSingleton<IDownloadTaskFactory<NovelItemViewModel, NovelDownloadTask>, NovelDownloadTaskFactory>()
            .AddSingleton(new LiteDatabase(AppInfo.DatabaseFilePath))
            .AddSingleton(provider => new DownloadHistoryPersistentManager(provider.GetRequiredService<LiteDatabase>(), App.AppViewModel.AppSettings.MaximumDownloadHistoryRecords))
            .AddSingleton(provider => new SearchHistoryPersistentManager(provider.GetRequiredService<LiteDatabase>(), App.AppViewModel.AppSettings.MaximumSearchHistoryRecords))
            .AddSingleton(provider => new BrowseHistoryPersistentManager(provider.GetRequiredService<LiteDatabase>(), App.AppViewModel.AppSettings.MaximumBrowseHistoryRecords))
            .AddSingleton(fileLogger)
            .BuildServiceProvider();
    }

    public async Task ShowExceptionDialogAsync(Exception e)
    {
        _ = await WindowFactory.RootWindow.Content.CreateAcknowledgementAsync(MiscResources.ExceptionEncountered, e.ToString());
    }

    public async Task InitializeAsync(bool activatedByProtocol)
    {
        _activatedByProtocol = activatedByProtocol;

        await AppKnownFolders.Temporary.ClearAsync();
        Cache = await FileCache.CreateDefaultAsync();

        AppServiceProvider = CreateServiceProvider();
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
        AppServiceProvider?.Dispose();
        DownloadManager?.Dispose();
        MakoClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}
