// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

#define DISABLE_XAML_GENERATED_BREAK_ON_UNHANDLED_EXCEPTION
#define DISABLE_XAML_GENERATED_RESOURCE_REFERENCE_DEBUG_OUTPUT
#define DISABLE_XAML_GENERATED_BINDING_DEBUG_OUTPUT

using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.AppLifecycle;
using Pixeval.Activation;
using Pixeval.AppManagement;
using Pixeval.Controls.Windowing;
using Pixeval.Pages.Login;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Mako.Model;
using Pixeval.Controls;
using WinUI3Utilities;

#if DEBUG
using System.Diagnostics;
#endif

namespace Pixeval;

public partial class App
{
    private const string ApplicationWideFontKey = "ContentControlThemeFontFamily";

    public App()
    {
        SettingsValueConverter.Context = SettingsSerializeContext.Default;
        AppViewModel = new AppViewModel(this);
        BookmarkTag.AllCountedTagString = MiscResources.AllCountedTagName;
        AppInfo.SetNameResolvers(AppViewModel.AppSettings);
        WindowFactory.Initialize(AppViewModel.AppSettings, AppInfo.IconApplicationUri, AppInfo.SvgIconApplicationUri);
        AppInstance.GetCurrent().Activated += (_, arguments) => ActivationRegistrar.Dispatch(arguments);
        InitializeComponent();
    }

    public static AppViewModel AppViewModel { get; private set; } = null!;

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        Resources["AppIdentifier"] = AppInfo.AppIdentifier;
        Resources["DefaultAppBarButtonStyle"].To<Style>().Setters[7] = new Setter(FrameworkElement.WidthProperty, 45);
        Resources["DefaultAppBarToggleButtonStyle"].To<Style>().Setters[8] = new Setter(FrameworkElement.WidthProperty, 45);

        if (AppViewModel.AppSettings.AppFontFamilyName.IsNotNullOrEmpty())
            Current.Resources[ApplicationWideFontKey] = new FontFamily(AppViewModel.AppSettings.AppFontFamilyName);

        AdvancedItemsView.ScrollRate = (float)AppViewModel.AppSettings.ScrollRate;

        var current = AppInstance.GetCurrent();
        var kind = current.GetActivatedEventArgs().Kind;
        switch (kind)
        {
            case ExtendedActivationKind.ToastNotification or ExtendedActivationKind.AppNotification:
                return;
            case ExtendedActivationKind.Protocol or ExtendedActivationKind.Launch when AppInstance.GetInstances().Count > 1:
            {
                var notCurrent = AppInstance.GetInstances().First(ins => !ins.IsCurrent);
                await notCurrent.RedirectActivationToAsync(current.GetActivatedEventArgs());
                // 一定要退出这个实例，否则主实例关闭后，整个应用不会退出
                Current.Exit();
                return;
            }
        }

        AppViewModel.Initialize();

        TabPage.CreatedWindowClosing += OnClosing;
        WindowFactory.Create(new LoginPage())
            .WithInitialized(OnLoaded)
            .WithClosing(OnClosing)
            .WithDestroying((_, _) => AppInfo.SaveContextWhenExit())
            .WithSizeLimit(800, 360)
            .Init(AppInfo.AppIdentifier, AppViewModel.AppSettings.WindowSize.ToSizeInt32(),
                AppViewModel.AppSettings.IsMaximized)
            .Activate();

        RegisterUnhandledExceptionHandler();
        return;

        static void OnLoaded(object s, RoutedEventArgs _)
        {
            //if (!AppViewModel.AppDebugTrace.ExitedSuccessfully
            //    && await w.PageContent.ShowDialogAsync(CheckExitedContentDialogResources.ContentDialogTitle,
            //        new CheckExitedDialog(),
            //        CheckExitedContentDialogResources.ContentDialogPrimaryButtonText,
            //        "",
            //        CheckExitedContentDialogResources.ContentDialogCloseButtonText) is ContentDialogResult.Primary)
            //{
            //    AppInfo.SaveContextWhenExit();
            //    w.Close();
            //    return;
            //}

            AppViewModel.AppDebugTrace.ExitedSuccessfully = false;
            AppInfo.SaveDebugTrace(AppViewModel.AppDebugTrace);
        }

        static async void OnClosing(AppWindow w, AppWindowClosingEventArgs e)
        {
            try
            {
                if (AppViewModel.AppSettings.ReconfirmationOfClosingWindow)
                {
                    e.Cancel = true;
                    var checkBox = new CheckBox
                    {
                        Content = ExitDialogResources.ReconfirmationOfClosingWindowCheckBoxContent
                    };
                    var window = WindowFactory.GetForkedWindows(w.Id.Value);
                    if (await window.Content.To<FrameworkElement>().CreateOkCancelAsync(
                            ExitDialogResources.ReconfirmationOfClosingWindowTitle,
                            checkBox) is ContentDialogResult.Primary)
                    {
                        AppViewModel.AppSettings.ReconfirmationOfClosingWindow = checkBox.IsChecked is false;
                        AppInfo.SaveConfig(AppViewModel.AppSettings);
                        window.Close();
                    }
                }
            }
            catch
            {
                e.Cancel = false;
            }
        }
    }

    private void RegisterUnhandledExceptionHandler()
    {
        DebugSettings.LayoutCycleTracingLevel = LayoutCycleTracingLevel.High;
        DebugSettings.LayoutCycleDebugBreakLevel = LayoutCycleDebugBreakLevel.High;
        DebugSettings.BindingFailed += (o, e) =>
        {
            var logger = AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>();
            logger.LogWarning(e.Message, null);
        };
        DebugSettings.XamlResourceReferenceFailed += (o, e) =>
        {
            var logger = AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>();
            logger.LogWarning(e.Message, null);
        };
        UnhandledException += (o, e) =>
        {
            var logger = AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>();
            logger.LogError(e.Message, e.Exception);
            e.Handled = true;
#if DEBUG
            if (Debugger.IsAttached)
                Debugger.Break();
#endif
        };
        TaskScheduler.UnobservedTaskException += (o, e) =>
        {
            var logger = AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>();
            logger.LogError(nameof(TaskScheduler.UnobservedTaskException), e.Exception);
            e.SetObserved();
#if DEBUG
            if (Debugger.IsAttached)
                Debugger.Break();
#endif
        };
        AppDomain.CurrentDomain.UnhandledException += (o, e) =>
        {
            var logger = AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>();
            if (e.IsTerminating)
                logger.LogCritical(nameof(AppDomain.UnhandledException), e.ExceptionObject as Exception);
            else
                logger.LogError(nameof(AppDomain.UnhandledException), e.ExceptionObject as Exception);
#if DEBUG
            if (Debugger.IsAttached)
                Debugger.Break();
            if (e.IsTerminating && Debugger.IsAttached)
                Debugger.Break();
#endif
        };
    }
}
