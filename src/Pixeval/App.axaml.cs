// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AvaDevTools;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Mako;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.AppManagement;
using Pixeval.I18N;
using Pixeval.Infrastructure;
using Pixeval.Models.Options;
using Pixeval.Models.Subscriptions;
using Pixeval.Themes;
using Pixeval.Utilities;
using Pixeval.Views.Home;
using Pixeval.Views.Login;
using Pixeval.Views.ViewContainers;

[assembly: InternalsVisibleTo("Pixeval.Tests")]

namespace Pixeval;

public class App : Application
{
    /// <summary>
    /// 确保随时能记录日志
    /// </summary>
    private FileLogger Logger { get; } = new(AppInfo.LogsFolder);

    private bool _allowExitWithActiveSubscriptionSync;
    private bool _isExitConfirmationOpen;
    private DateTimeOffset _rateLimitNotificationUntil = DateTimeOffset.MinValue;

    public override void Initialize()
    {
        RegisterUnhandledExceptionHandler();
        I18NManager.CandidatePaths.Add(AppInfo.ExtensionsFolder);
        I18NManager.Register(new JsonMarkdownLangPlugin(), LanguageHelper.DefaultLanguage);
        AppViewModel = new AppViewModel(this, Logger);
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = LanguageHelper.FindClosest(AppViewModel.AppSettings.ApplicationSettings.CultureName);
        I18NManager.Initialize();
        AppViewModel.InitializeProvider();
        AppViewModel.MakoClient.RateLimitEncountered += MakoClient_OnRateLimitEncountered;

        AvaloniaXamlLoader.Load(this);

        // Windows 11 Mica 观感按环境选用：仅当 Mica 实际可用（Win11 + 系统「透明效果」开启）
        // 才合并半透明表面覆写。Win10 / Linux / 关闭透明效果时保持 Brushes.axaml 的纯色观感。
        if (MicaWindowHelper.IsMicaEnabled())
        {
            Resources.MergedDictionaries.Add(new MicaStyles());
            // 给菜单/Flyout/ToolTip/下拉框弹出层挂 DWM 亚克力，随主题模糊+染色。
            MicaWindowHelper.EnableAcrylicPopups();
        }

        ApplyAppFontFamily(AppViewModel.AppSettings.ApplicationSettings.AppFontFamily);
        RequestedThemeVariant = AppViewModel.AppSettings.ApplicationSettings.Theme switch
        {
            ApplicationTheme.Light => ThemeVariant.Light,
            ApplicationTheme.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };

#if DEBUG
        this.AttachAvaDevTools();
#endif
    }

    public static AppViewModel AppViewModel { get; private set; } = null!;

    public static void ApplyAppFontFamily(IEnumerable<string> fontFamilies)
    {
        var resources = Current!.Resources;
        resources["ContentControlThemeFontFamily"] = FontFamilyHelper.Create(fontFamilies) ?? FontFamily.Default;
    }

    internal void RegisterWindow(Window window)
    {
        window.Closing += async (_, args) =>
        {
            if (_allowExitWithActiveSubscriptionSync
                || !IsLastVisibleWindow(window)
                || AppViewModel.AppServiceProvider.GetService<WorkSubscriptionDownloadService>() is not { IsSyncInProgress: true } service)
                return;

            args.Cancel = true;
            if (_isExitConfirmationOpen || window.Content is not ViewContainerBase viewContainer)
                return;

            _isExitConfirmationOpen = true;
            try
            {
                var result = await viewContainer.CreateOkCancelAsync(
                    I18NManager.GetResource(WorkSubscriptionsSettingsExpanderResources.ExitConfirmation.Title),
                    I18NManager.GetResource(WorkSubscriptionsSettingsExpanderResources.ExitConfirmation.Content));
                if (result is not Controls.ContentDialogResult.Primary)
                    return;

                await service.CancelAndWaitAsync();
                _allowExitWithActiveSubscriptionSync = true;
                try
                {
                    window.Close();
                }
                finally
                {
                    _allowExitWithActiveSubscriptionSync = false;
                }
            }
            finally
            {
                _isExitConfirmationOpen = false;
            }
        };
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var viewContainer = new TabViewContainer();

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.ShutdownMode = ShutdownMode.OnLastWindowClose;
                desktop.Exit += async static (o, e) =>
                {
                    AppInfo.SaveContext();
                    await AppViewModel.DisposeAsync();
                };

                viewContainer.SetInterTabController(true);
#if PIXEVAL_MCP
                _ = AppViewModel.AppServiceProvider.GetService<IPixevalMcpService>()?.StartAsync();
#endif

                // 这个窗口可能会被用户关闭，所以不设为desktop.MainWindow
                new Window { Content = viewContainer }
                    .Init(
                        AppInfo.AppIdentifier,
                        AppInfo.IconApplicationUri,
                        AppViewModel.AppSettings.ApplicationSettings.WindowWidth,
                        AppViewModel.AppSettings.ApplicationSettings.WindowHeight,
                        800,
                        450,
                        AppViewModel.AppSettings.ApplicationSettings.IsMaximized).Show();

                break;
            case ISingleViewApplicationLifetime singleViewPlatform:
                viewContainer.SetInterTabController(false);
                singleViewPlatform.MainView = viewContainer;
                break;
        }

        _ = LoginAsync(viewContainer);

        base.OnFrameworkInitializationCompleted();
    }

    private static async Task LoginAsync(ViewContainerBase viewContainer)
    {
        try
        {
            if (AppViewModel.GetCurrentLoginUser() is { RefreshToken: { } refreshToken }
                && !string.IsNullOrWhiteSpace(refreshToken))
            {
                await AppViewModel.MakoClient.SetTokenAsync(refreshToken);
                if (await AppViewModel.MakoClient.IdentifyTokenAsync())
                {
                    viewContainer.NavigateTo(new HomePage());
                    AppViewModel.QueueWorkSubscriptionSyncAll();
                    return;
                }

                viewContainer.ShowError(I18NManager.GetResource(MainPageResources.LoggingIn.Failed));
            }
        }
        catch (Exception e)
        {
            AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>()
                .LogError(nameof(LoginAsync), e);
            viewContainer.ShowError(I18NManager.GetResource(MainPageResources.LoggingIn.Failed));
        }

        viewContainer.NavigateTo(new LoginPage());
    }

    private bool IsLastVisibleWindow(Window window) =>
        ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
        && desktop.Windows.Where(static candidate => candidate.IsVisible).All(candidate => ReferenceEquals(candidate, window));

    private void MakoClient_OnRateLimitEncountered(MakoClient sender, RateLimitEventArgs args) =>
        Dispatcher.UIThread.Post(() =>
        {
            var now = DateTimeOffset.UtcNow;
            if (now < _rateLimitNotificationUntil)
                return;

            _rateLimitNotificationUntil = args.RetryAt > now
                ? args.RetryAt
                : now.AddSeconds(30);
            var viewContainer = ApplicationLifetime switch
            {
                IClassicDesktopStyleApplicationLifetime desktop => desktop.Windows
                    .OrderByDescending(static window => window.IsActive)
                    .Select(static window => window.Content)
                    .OfType<ViewContainerBase>()
                    .FirstOrDefault(),
                ISingleViewApplicationLifetime singleView => singleView.MainView as ViewContainerBase,
                _ => null
            };
            viewContainer?.ShowWarning(
                I18NManager.GetResource(WorkSubscriptionsSettingsExpanderResources.RateLimitNotification.Title),
                I18NManager.GetResource(WorkSubscriptionsSettingsExpanderResources.RateLimitNotification.Content));
        });

    private void RegisterUnhandledExceptionHandler()
    {
        Dispatcher.UIThread.UnhandledException += (_, e) =>
        {
            if (e.Exception is OutOfMemoryException or StackOverflowException or AccessViolationException)
            {
                Logger.LogCritical(nameof(Dispatcher.UnhandledException), e.Exception);
                return;
            }

            // Avalonia event handlers are necessarily async void; keep one final boundary on the UI dispatcher.
            Logger.LogError(nameof(Dispatcher.UnhandledException), e.Exception);
            e.Handled = true;
        };
        TaskScheduler.UnobservedTaskException += (o, e) =>
        {
            Logger.LogError(nameof(TaskScheduler.UnobservedTaskException), e.Exception);
            e.SetObserved();
        };
        AppDomain.CurrentDomain.UnhandledException += (o, e) =>
        {
            if (e.IsTerminating)
                Logger.LogCritical(nameof(AppDomain.UnhandledException), e.ExceptionObject as Exception);
            else
                Logger.LogError(nameof(AppDomain.UnhandledException), e.ExceptionObject as Exception);
        };
    }
}
