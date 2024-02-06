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
using Microsoft.Extensions.Hosting;
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
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval;

public class AppViewModel(App app)
{
    private bool _activatedByProtocol;

    public IHost AppHost { get; private set; } = null!;

    public IServiceScope AppServicesScope => AppHost.Services.CreateScope();

    /// <summary>
    /// Indicates whether the exit is caused by a logout action
    /// </summary>
    public bool SignOutExit { get; set; }

    public App App { get; } = app;

    public DownloadManager<IllustrationDownloadTask> DownloadManager { get; private set; } = null!;

    public MakoClient MakoClient { get; set; } = null!; // The null-state of MakoClient is transient

    public AppSetting AppSetting { get; set; } = null!;

    public FileCache Cache { get; private set; } = null!;

    public long PixivUid => MakoClient.Session.Id;

    public void AppLoggedIn()
    {
        DownloadManager = new DownloadManager<IllustrationDownloadTask>(AppSetting.MaxDownloadTaskConcurrencyLevel, MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi));
        AppContext.RestoreHistories();
    }

    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
                services.AddSingleton<IDownloadTaskFactory<IllustrationItemViewModel, IllustrationDownloadTask>, IllustrationDownloadTaskFactory>()
                    .AddSingleton(new LiteDatabase(AppContext.DatabaseFilePath))
                    .AddSingleton(provider => new DownloadHistoryPersistentManager(provider.GetRequiredService<LiteDatabase>(), App.AppViewModel.AppSetting.MaximumDownloadHistoryRecords))
                    .AddSingleton(provider => new SearchHistoryPersistentManager(provider.GetRequiredService<LiteDatabase>(), App.AppViewModel.AppSetting.MaximumSearchHistoryRecords))
                    .AddSingleton(provider => new BrowseHistoryPersistentManager(provider.GetRequiredService<LiteDatabase>(), App.AppViewModel.AppSetting.MaximumBrowseHistoryRecords))
                    .AddSingleton(_ => new FileLogger(ApplicationData.Current.LocalFolder.Path + @"\Logs\"))
                );
    }

    public async Task ShowExceptionDialogAsync(Exception e)
    {
        _ = await WindowFactory.RootWindow.Content.CreateAcknowledgementAsync(MiscResources.ExceptionEncountered, e.ToString());
    }

    public async Task InitializeAsync(bool activatedByProtocol)
    {
        _activatedByProtocol = activatedByProtocol;

        AppHost = CreateHostBuilder().Build();

        await AppKnownFolders.Temporary.ClearAsync();
        Cache = await FileCache.CreateDefaultAsync();

        _ = AppHost.RunAsync();
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
}
