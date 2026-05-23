// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Pixeval.AppManagement;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.Capability;
using Pixeval.Views.Login;
using Pixeval.Views.ViewContainers;

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
        I18NManager.Register(new JsonMarkdownLangPlugin(), LanguageHelper.DefaultLanguage);
        AppViewModel = new AppViewModel(this, Logger);
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = LanguageHelper.FindClosest(AppViewModel.AppSettings.CultureName);
        I18NManager.Initialize();
        AppViewModel.InitializeProvider();
        
        AvaloniaXamlLoader.Load(this);
        Resources["ContentControlThemeFontFamily"] = new FontFamily(string.Join(',', AppViewModel.AppSettings.AppFontFamily));

#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public static AppViewModel AppViewModel { get; private set; } = null!;

    public override void OnFrameworkInitializationCompleted()
    {
        ViewContainerBase? viewContainer = null;

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.Exit += (o, e) =>
                {
                    AppInfo.SaveContext();
                    AppViewModel.Dispose();
                };

                viewContainer = new TabViewContainer();
                // 这个窗口可能会被用户关闭，所以不设为desktop.MainWindow
                new Window { Content = viewContainer }
                    .Init(
                        AppInfo.AppIdentifier,
                        AppInfo.IconApplicationUri,
                        AppViewModel.AppSettings.WindowWidth,
                        AppViewModel.AppSettings.WindowHeight,
                        800,
                        450,
                        AppViewModel.AppSettings.IsMaximized).Show();

                break;
            case ISingleViewApplicationLifetime singleViewPlatform:
                singleViewPlatform.MainView = viewContainer = new SingleViewContainer
                {
                    DataContext = new MainViewModel()
                };
                break;
        }

        if (viewContainer is not null)
        {
            _ = LoginAsync(viewContainer);
        }

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
                viewContainer.NavigateTo(new RecommendWorksPage());
                AppViewModel.QueueWorkSubscriptionSyncAll();
                return;
            }
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
