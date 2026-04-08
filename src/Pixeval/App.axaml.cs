using System;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Rendering;
using LiveMarkdown.Avalonia;
using Pixeval.AppManagement;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views;
using Pixeval.Views.Capability;
using Pixeval.Views.ViewContainers;
using Pixeval.Views.Viewers;

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
        AppViewModel = new AppViewModel(this, Logger);
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = LanguageHelper.FindClosest(AppViewModel.AppSettings.CultureName);
        I18NManager.Register(new JsonMarkdownLangPlugin(), LanguageHelper.DefaultLanguage);
        AppViewModel.Initialize();
        AppViewModel.InitializeProvider();
        
        AvaloniaXamlLoader.Load(this);

        AsyncImageLoader.DefaultDecoders =
        [
            SvgImageDecoder.Shared,
            DefaultBitmapDecoder.Shared
        ];
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
                    AppInfo.Dispose();
                };

                viewContainer = new TabViewContainer();
                desktop.MainWindow = new Window { Content = viewContainer }
                    .Init(
                        AppInfo.AppIdentifier,
                        AppInfo.IconApplicationUri,
                        AppViewModel.AppSettings.WindowWidth,
                        AppViewModel.AppSettings.WindowHeight,
                        800,
                        450,
                        AppViewModel.AppSettings.IsMaximized);

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
        var loginContext = AppViewModel.LoginContext;
        var token = loginContext.CurrentRefreshToken;
        if (!string.IsNullOrWhiteSpace(token)
            && loginContext.Users.ContainsKey(token))
        {
            AppViewModel.MakoClient.SetToken(token);
            if (await AppViewModel.MakoClient.IdentifyTokenAsync())
            {
                viewContainer.NavigateTo<RecommendWorksPage>();
                viewContainer.NavigateTo(typeof(WorkViewerSplitView), null, "header");
                return;
            }
        }
        viewContainer.NavigateTo<LoginPage>();
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
