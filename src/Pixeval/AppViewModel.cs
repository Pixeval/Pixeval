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
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics;
using CommunityToolkit.WinUI;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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
using AppContext = Pixeval.AppManagement.AppContext;
using ApplicationTheme = Pixeval.Options.ApplicationTheme;

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

    public MainWindow Window { get; private set; } = null!;

    public AppWindow AppWindow { get; private set; } = null!;

    public DownloadManager<ObservableDownloadTask> DownloadManager { get; private set; } = null!;

    public Frame AppWindowRootFrame => Window.PixevalAppRootFrame;

    public MakoClient MakoClient { get; set; } = null!; // The null-state of MakoClient is transient

    public AppSetting AppSetting { get; set; } = null!;

    public FileCache Cache { get; private set; } = null!;

    public ElementTheme AppRootFrameTheme => AppWindowRootFrame.RequestedTheme;

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

    public IntPtr GetMainWindowHandle()
    {
        return Window.GetWindowHandle();
    }

    public void SwitchTheme(ApplicationTheme theme)
    {
        Window.PixevalAppRootFrame.RequestedTheme = theme switch
        {
            ApplicationTheme.Dark => ElementTheme.Dark,
            ApplicationTheme.Light => ElementTheme.Light,
            ApplicationTheme.SystemDefault => ElementTheme.Default,
            _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, null)
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

    private void RegisterUnhandledExceptionHandler()
    {
        App.UnhandledException += async (_, args) =>
        {
            args.Handled = true;
            await Window.DispatcherQueue.EnqueueAsync(async () => await UncaughtExceptionHandler(args.Exception));
        };

        TaskScheduler.UnobservedTaskException += async (_, args) =>
        {
            args.SetObserved();
            await Window.DispatcherQueue.EnqueueAsync(async () => await UncaughtExceptionHandler(args.Exception));
        };

        AppDomain.CurrentDomain.UnhandledException += async (_, args) =>
        {
            if (args.ExceptionObject is Exception e)
            {
                await Window.DispatcherQueue.EnqueueAsync(async () => await UncaughtExceptionHandler(e));
            }
            else
            {
                ExitWithPushedNotification();
            }
        };

#if DEBUG
        // ReSharper disable once UnusedParameter.Local
        static Task UncaughtExceptionHandler(Exception e)
        {
            Debugger.Break();
            return Task.CompletedTask;
        }
#elif RELEASE
            Task UncaughtExceptionHandler(Exception e)
            {
                return ShowExceptionDialogAsync(e);
            }
#endif
    }

    public async Task ShowExceptionDialogAsync(Exception e)
    {
        await MessageDialogBuilder.CreateAcknowledgement(Window, MiscResources.ExceptionEncountered, e.ToString()).ShowAsync();
    }

    public void DispatchTask(DispatcherQueueHandler action)
    {
        Window.DispatcherQueue.TryEnqueue(action);
    }

    public Task DispatchTaskAsync(Func<Task> action)
    {
        return Window.DispatcherQueue.EnqueueAsync(action);
    }

    public Task<T> DispatchTaskAsync<T>(Func<Task<T>> action)
    {
        return Window.DispatcherQueue.EnqueueAsync(action);
    }

    /// <summary>
    ///     Exit the notification after pushing an <see cref="ApplicationExitingMessage" />
    ///     to the <see cref="EventChannel" />
    /// </summary>
    /// <returns></returns>
    public void ExitWithPushedNotification()
    {
        WeakReferenceMessenger.Default.Send(new ApplicationExitingMessage());
        Application.Current.Exit();
    }

    public async Task InitializeAsync(bool activatedByProtocol)
    {
        _activatedByProtocol = activatedByProtocol;

        AppHost = CreateHostBuilder().Build();

        RegisterUnhandledExceptionHandler();
        await AppContext.WriteLogoIcoIfNotExist();

        Window = new MainWindow();
        AppWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(GetMainWindowHandle()));
        AppWindow.Title = AppContext.AppIdentifier;
        AppWindow.Resize(new SizeInt32(AppSetting.WindowWidth, AppSetting.WindowHeight));
        AppWindow.Show();
        AppWindow.SetIcon(await AppContext.GetIconAbsolutePath());

        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            var titleBar = App.AppViewModel.AppWindow.TitleBar;
            titleBar.ExtendsContentIntoTitleBar = true;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonHoverBackgroundColor = ((SolidColorBrush) App.Resources["SystemControlBackgroundBaseLowBrush"]).Color;
            titleBar.ButtonForegroundColor = ((SolidColorBrush)App.Resources["SystemControlForegroundBaseHighBrush"]).Color;
        }

        App.TryApplyMica();

        // Window.ExtendsContentIntoTitleBar = true;
        // Window.SetTitleBar(Window.CustomTitleBar);

        await AppKnownFolders.Temporary.ClearAsync();
        Cache = await FileCache.CreateDefaultAsync();

        AppHost.RunAsync().Discard();
    }

    public (int, int) GetAppWindowSizeTuple()
    {
        var windowSize = AppWindow.Size;
        return (windowSize.Width, windowSize.Height);
    }

    public Size GetAppWindowSize()
    {
        return AppWindow.Size.ToWinRtSize();
    }

    public Size GetDpiAwareAppWindowSize()
    {
        var dpi = User32.GetDpiForWindow(GetMainWindowHandle());
        var size = GetAppWindowSize();
        var scalingFactor = (float)dpi / 96;
        return new Size(size.Width / scalingFactor, size.Height / scalingFactor);
    }

    public (int, int) GetDpiAwareAppWindowSizeTuple()
    {
        var size = GetDpiAwareAppWindowSize();
        return ((int, int))(size.Width, size.Height);
    }

    public void PrepareForActivation()
    {
        Window.ShowProgressRing();
    }

    public void ActivationProcessed()
    {
        Window.HideProgressRing();
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

    public void ShowSnack(string text, int duration)
    {
        Window.PixevalAppSnackBar.Show(text, duration);
    }
}
