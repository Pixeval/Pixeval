// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Pixeval.AppManagement;
using Pixeval.I18N;
using Pixeval.Models.Options;
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

    public override void Initialize()
    {
        RegisterUnhandledExceptionHandler();
        I18NManager.CandidatePaths.Add(AppInfo.ExtensionsFolder);
        I18NManager.Register(new JsonMarkdownLangPlugin(), LanguageHelper.DefaultLanguage);
        AppViewModel = new AppViewModel(this, Logger);
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = LanguageHelper.FindClosest(AppViewModel.AppSettings.ApplicationSettings.CultureName);
        I18NManager.Initialize();
        AppViewModel.InitializeProvider();
        
        AvaloniaXamlLoader.Load(this);
        Resources["ContentControlThemeFontFamily"] = new FontFamily(string.Join(',', AppViewModel.AppSettings.ApplicationSettings.AppFontFamily));
        RequestedThemeVariant = AppViewModel.AppSettings.ApplicationSettings.Theme switch
        {
            ApplicationTheme.Light => ThemeVariant.Light,
            ApplicationTheme.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };

#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public static AppViewModel AppViewModel { get; private set; } = null!;

    public override void OnFrameworkInitializationCompleted()
    {
        var viewContainer = new TabViewContainer();

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.Exit += (o, e) =>
                {
                    AppInfo.SaveContext();
                    AppViewModel.Dispose();
                };

                viewContainer.SetInterTabController(true);

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
        if (AppViewModel.GetCurrentLoginUser() is { RefreshToken: { } token }
            && !string.IsNullOrWhiteSpace(token))
        {
            AppViewModel.MakoClient.SetToken(token);
            if (await AppViewModel.MakoClient.IdentifyTokenAsync())
            {
                viewContainer.NavigateTo(new HomePage());
                AppViewModel.QueueWorkSubscriptionSyncAll();
                return;
            }

            viewContainer.ShowError(I18NManager.GetResource(MainPageResources.LoggingInFailed));
        }
        viewContainer.NavigateTo(new LoginPage());
    }

    private void RegisterUnhandledExceptionHandler()
    {
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
