#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/App.xaml.cs
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
#define DISABLE_XAML_GENERATED_BREAK_ON_UNHANDLED_EXCEPTION
#define DISABLE_XAML_GENERATED_RESOURCE_REFERENCE_DEBUG_OUTPUT
#define DISABLE_XAML_GENERATED_BINDING_DEBUG_OUTPUT

using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.AppLifecycle;
using Pixeval.Activation;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.Pages.Login;
using WinUI3Utilities;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Logging;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Pixeval.Controls.DialogContent;
using Pixeval.CoreApi.Model;

#if DEBUG
using System.Diagnostics;
#endif

namespace Pixeval;

public partial class App
{
    private const string ApplicationWideFontKey = "ContentControlThemeFontFamily";

    public App()
    {
        AppViewModel = new AppViewModel(this);
        BookmarkTag.AllCountedTagString = MiscResources.AllCountedTagName;
        AppInfo.SetNameResolvers(AppViewModel.AppSettings);
        WindowFactory.Initialize(AppViewModel.AppSettings, AppInfo.IconAbsolutePath);
        AppInstance.GetCurrent().Activated += (_, arguments) => ActivationRegistrar.Dispatch(arguments);
        InitializeComponent();
    }

    public static AppViewModel AppViewModel { get; private set; } = null!;

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        Resources["DefaultAppBarButtonStyle"].To<Style>().Setters[7] = new Setter(FrameworkElement.WidthProperty, 45);
        Resources["DefaultAppBarToggleButtonStyle"].To<Style>().Setters[8] = new Setter(FrameworkElement.WidthProperty, 45);

        if (AppInstance.GetCurrent().GetActivatedEventArgs().Kind is ExtendedActivationKind.ToastNotification)
            return;

        var isProtocolActivated = AppInstance.GetCurrent().GetActivatedEventArgs() is { Kind: ExtendedActivationKind.Protocol };
        if (isProtocolActivated && AppInstance.GetInstances().Count > 1)
        {
            var notCurrent = AppInstance.GetInstances().First(ins => !ins.IsCurrent);
            await notCurrent.RedirectActivationToAsync(AppInstance.GetCurrent().GetActivatedEventArgs());
            return;
        }

        if (AppViewModel.AppSettings.AppFontFamilyName.IsNotNullOrEmpty())
            Current.Resources[ApplicationWideFontKey] = new FontFamily(AppViewModel.AppSettings.AppFontFamilyName);
        await AppKnownFolders.InitializeAsync();

        await AppViewModel.InitializeAsync(isProtocolActivated);

        WindowFactory.Create(out var w)
            .WithLoaded(onLoaded: OnLoaded)
            .WithClosing((_, _) => AppInfo.SaveContextWhenExit()) // TODO: 从运行打开应用的时候不会ExitApp，就算是调用App.Current.Exit();
            .WithSizeLimit(800, 360)
            .Init(AppInfo.AppIdentifier, AppViewModel.AppSettings.WindowSize.ToSizeInt32(), AppViewModel.AppSettings.IsMaximized)
            .Activate();

        RegisterUnhandledExceptionHandler();
        return;

        async void OnLoaded(object s, RoutedEventArgs _)
        {
            if (!AppViewModel.AppDebugTrace.ExitedSuccessfully
                && await w.Content.ShowContentDialogAsync(CheckExitedContentDialogResources.ContentDialogTitle,
                    new CheckExitedDialog(),
                    CheckExitedContentDialogResources.ContentDialogPrimaryButtonText,
                    "",
                    CheckExitedContentDialogResources.ContentDialogCloseButtonText) is ContentDialogResult.Primary)
            {
                AppInfo.SaveContextWhenExit();
                w.Close();
                return;
            }

            AppViewModel.AppDebugTrace.ExitedSuccessfully = false;
            AppInfo.SaveDebugTrace(AppViewModel.AppDebugTrace);

            s.To<Frame>().NavigateTo<LoginPage>(w.HWnd);
        }
    }

    private void RegisterUnhandledExceptionHandler()
    {
        DebugSettings.BindingFailed += (o, e) =>
        {
            using var scope = AppViewModel.AppServicesScope;
            var logger = scope.ServiceProvider.GetRequiredService<FileLogger>();
            logger.LogWarning(e.Message, null);
        };
        DebugSettings.XamlResourceReferenceFailed += (o, e) =>
        {
            using var scope = AppViewModel.AppServicesScope;
            var logger = scope.ServiceProvider.GetRequiredService<FileLogger>();
            logger.LogWarning(e.Message, null);
        };
        UnhandledException += (o, e) =>
        {
            using var scope = AppViewModel.AppServicesScope;
            var logger = scope.ServiceProvider.GetRequiredService<FileLogger>();
            logger.LogError(e.Message, e.Exception);
            e.Handled = true;
#if DEBUG
            if (Debugger.IsAttached)
                Debugger.Break();
#endif
        };
        TaskScheduler.UnobservedTaskException += (o, e) =>
        {
            using var scope = AppViewModel.AppServicesScope;
            var logger = scope.ServiceProvider.GetRequiredService<FileLogger>();
            logger.LogError(nameof(TaskScheduler.UnobservedTaskException), e.Exception);
            e.SetObserved();
#if DEBUG
            if (Debugger.IsAttached)
                Debugger.Break();
#endif
        };
        AppDomain.CurrentDomain.UnhandledException += (o, e) =>
        {
            using var scope = AppViewModel.AppServicesScope;
            var logger = scope.ServiceProvider.GetRequiredService<FileLogger>();
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
