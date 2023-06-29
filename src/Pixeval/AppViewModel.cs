#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/AppViewModel.cs
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
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.AppManagement;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Net;
using Pixeval.Database.Managers;
using Pixeval.Download;
using Pixeval.Messages;
using Pixeval.Util.IO;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using WinUI3Utilities;
using AppContext = Pixeval.AppManagement.AppContext;
using AppTheme = Pixeval.Options.ApplicationTheme;
using Microsoft.UI;
using Pixeval.UserControls.IllustrationView;

namespace Pixeval;

public class AppViewModel : AutoActivateObservableRecipient,
    IRecipient<ApplicationExitingMessage>,
    IRecipient<LoginCompletedMessage>
{
    private bool _activatedByProtocol;

    public AppViewModel(App app)
    {
        App = app;
    }

    public IHost AppHost { get; private set; } = null!;

    public IServiceScope AppServicesScope => AppHost.Services.CreateScope();

    /// <summary>
    /// Indicates whether the exit is caused by a logout action
    /// </summary>
    public bool SignOutExit { get; set; }

    public App App { get; }

    public DownloadManager<ObservableDownloadTask> DownloadManager { get; private set; } = null!;

    public MakoClient MakoClient { get; set; } = null!; // The null-state of MakoClient is transient

    public AppSetting AppSetting { get; set; } = null!;

    public FileCache Cache { get; private set; } = null!;

    public ElementTheme AppRootFrameTheme => AppWindowRootFrame.RequestedTheme;

    public Frame AppWindowRootFrame => ((MainWindow)CurrentContext.Window).PixevalAppRootFrame;

    public string? PixivUid => MakoClient.Session.Id;

    public void Receive(ApplicationExitingMessage message)
    {
        AppContext.SaveContext();
    }

    public void Receive(LoginCompletedMessage message)
    {
        DownloadManager = new DownloadManager<ObservableDownloadTask>(AppSetting.MaxDownloadTaskConcurrencyLevel, MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi));
        AppContext.RestoreHistories();
    }

    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
                services.AddSingleton<IDownloadTaskFactory<IllustrationViewModel, ObservableDownloadTask>, IllustrationDownloadTaskFactory>()
                    .AddSingleton(new LiteDatabase(AppContext.DatabaseFilePath))
                    .AddSingleton(provider => new DownloadHistoryPersistentManager(provider.GetRequiredService<LiteDatabase>(), App.AppViewModel.AppSetting.MaximumDownloadHistoryRecords))
                    .AddSingleton(provider => new SearchHistoryPersistentManager(provider.GetRequiredService<LiteDatabase>(), App.AppViewModel.AppSetting.MaximumSearchHistoryRecords))
                    .AddSingleton(provider => new BrowseHistoryPersistentManager(provider.GetRequiredService<LiteDatabase>(), App.AppViewModel.AppSetting.MaximumBrowseHistoryRecords)));
    }

    public void SwitchTheme(AppTheme theme)
    {
        var selectedTheme = theme switch
        {
            AppTheme.Dark => ElementTheme.Dark,
            AppTheme.Light => ElementTheme.Light,
            AppTheme.SystemDefault => ElementTheme.Default,
            _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, null)
        };

        if (CurrentContext.Window.Content is FrameworkElement rootElement)
            rootElement.RequestedTheme = selectedTheme;
        
    }

    public async Task ShowExceptionDialogAsync(Exception e)
    {
        await MessageDialogBuilder.CreateAcknowledgement(CurrentContext.Window, MiscResources.ExceptionEncountered, e.ToString()).ShowAsync();
    }

    public async Task InitializeAsync(bool activatedByProtocol)
    {
        _activatedByProtocol = activatedByProtocol;

        AppHost = CreateHostBuilder().Build();

        await AppContext.WriteLogoIcoIfNotExist();

        await AppKnownFolders.Temporary.ClearAsync();
        Cache = await FileCache.CreateDefaultAsync();

        AppHost.RunAsync().Discard();
    }

    public void PrepareForActivation()
    {
        ((MainWindow)CurrentContext.Window).ShowProgressRing();
    }

    public void ActivationProcessed()
    {
        ((MainWindow)CurrentContext.Window).HideProgressRing();
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
