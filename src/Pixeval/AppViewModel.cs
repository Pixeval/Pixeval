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
using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics;
using ABI.Windows.System;
using CommunityToolkit.WinUI;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using PInvoke;
using Pixeval.AppManagement;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Net;
using Pixeval.Database.Managers;
using Pixeval.Download;
using Pixeval.Messages;
using Pixeval.UserControls;
using Pixeval.Util.IO;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using WinUI3Utilities;
using AppContext = Pixeval.AppManagement.AppContext;
using ApplicationTheme = Pixeval.Options.ApplicationTheme;
using Microsoft.UI;
using DispatcherQueueHandler = Microsoft.UI.Dispatching.DispatcherQueueHandler;

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

    public void SwitchTheme(ApplicationTheme theme)
    {
        var selectedTheme = theme switch
        {
            ApplicationTheme.Dark => ElementTheme.Dark,
            ApplicationTheme.Light => ElementTheme.Light,
            ApplicationTheme.SystemDefault => ElementTheme.Default,
            _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, null)
        };

        if (CurrentContext.Window.Content is FrameworkElement rootElement)
            rootElement.RequestedTheme = selectedTheme;

        // TODO: 没反应
        Application.Current.Resources["WindowCaptionForeground"] =
            selectedTheme switch
            {
                ElementTheme.Dark => Colors.White,
                ElementTheme.Light => Colors.Black,
                _ => Application.Current.RequestedTheme is Microsoft.UI.Xaml.ApplicationTheme.Dark ? Colors.White : Colors.Black
            };
    }

    public void RootFrameNavigate(Type type, object parameter, NavigationTransitionInfo infoOverride)
    {
        AppWindowRootFrame.Navigate(type, parameter, infoOverride);
    }

    public void RootFrameNavigate(Type type, object parameter)
    {
        AppWindowRootFrame.Navigate(type, parameter);
    }

    public void RootFrameNavigate(Type type)
    {
        AppWindowRootFrame.Navigate(type);
    }

    public async Task ShowExceptionDialogAsync(Exception e)
    {
        await MessageDialogBuilder.CreateAcknowledgement(CurrentContext.Window, MiscResources.ExceptionEncountered, e.ToString()).ShowAsync();
    }

    public void DispatchTask(DispatcherQueueHandler action)
    {
        CurrentContext.Window.DispatcherQueue.TryEnqueue(action);
    }

    public Task DispatchTaskAsync(Func<Task> action)
    {
        return CurrentContext.Window.DispatcherQueue.EnqueueAsync(action);
    }

    public Task<T> DispatchTaskAsync<T>(Func<Task<T>> action)
    {
        return CurrentContext.Window.DispatcherQueue.EnqueueAsync(action);
    }

    public async Task InitializeAsync(bool activatedByProtocol)
    {
        _activatedByProtocol = activatedByProtocol;

        AppHost = CreateHostBuilder().Build();

        await AppContext.WriteLogoIcoIfNotExist();
        CurrentContext.IconPath = await AppContext.GetIconAbsolutePath();
        CurrentContext.Window = new MainWindow();
        CurrentContext.Title = AppContext.AppIdentifier;

        AppHelper.Initialize(new SizeInt32(AppSetting.WindowWidth, AppSetting.WindowHeight));

        await AppKnownFolders.Temporary.ClearAsync();
        Cache = await FileCache.CreateDefaultAsync();

        AppHost.RunAsync().Discard();
    }

    public (int, int) GetAppWindowSizeTuple()
    {
        var windowSize = CurrentContext.AppWindow.Size;
        return (windowSize.Width, windowSize.Height);
    }

    public Size GetAppWindowSize()
    {
        return CurrentContext.AppWindow.Size.ToWinRtSize();
    }

    public Size GetDpiAwareAppWindowSize()
    {
        var dpi = User32.GetDpiForWindow(CurrentContext.HWnd);
        var size = GetAppWindowSize();
        var scalingFactor = (float)dpi / 96;
        return new Size(size.Width / scalingFactor, size.Height / scalingFactor);
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

    public async void ShowSnack(string text, int duration)
    {
        var window = (MainWindow)CurrentContext.Window;

        window.PixevalAppSnackBar.Title = text;

        window.PixevalAppSnackBar.IsOpen = true;
        await Task.Delay(duration);
        window.PixevalAppSnackBar.IsOpen = false;
    }
}
