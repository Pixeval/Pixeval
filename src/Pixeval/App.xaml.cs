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
using AppContext = Pixeval.AppManagement.AppContext;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Logging;
using Pixeval.Util.UI;

namespace Pixeval;

public partial class App
{
    private const string ApplicationWideFontKey = "ContentControlThemeFontFamily";

    public App()
    {
        AppViewModel = new AppViewModel(this) { AppSetting = AppContext.LoadConfig() ?? new AppSetting() };
        AppContext.SetNameResolver(AppViewModel.AppSetting);
        WindowFactory.Initialize(AppViewModel.AppSetting, AppContext.IconAbsolutePath);
        AppInstance.GetCurrent().Activated += (_, arguments) => ActivationRegistrar.Dispatch(arguments);
        InitializeComponent();
    }

    public static AppViewModel AppViewModel { get; private set; } = null!;

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        if (AppInstance.GetCurrent().GetActivatedEventArgs().Kind is ExtendedActivationKind.ToastNotification)
        {
            return;
        }

        var isProtocolActivated = AppInstance.GetCurrent().GetActivatedEventArgs() is { Kind: ExtendedActivationKind.Protocol };
        if (isProtocolActivated && AppInstance.GetInstances().Count > 1)
        {
            var notCurrent = AppInstance.GetInstances().First(ins => !ins.IsCurrent);
            await notCurrent.RedirectActivationToAsync(AppInstance.GetCurrent().GetActivatedEventArgs());
            return;
        }

        Current.Resources[ApplicationWideFontKey] = new FontFamily(AppViewModel.AppSetting.AppFontFamilyName);
        await AppKnownFolders.InitializeAsync();

        WindowFactory.Create(out var w)
            .WithLoaded((s, _) => s.To<Frame>().NavigateTo<LoginPage>(w))
            .WithClosing((_, _) => AppContext.SaveContext()) // TODO: 从运行打开应用的时候不会ExitApp，就算是调用App.Current.Exit();
            .WithSizeLimit(800, 360)
            .Init(AppContext.AppIdentifier, AppViewModel.AppSetting.WindowSize.ToSizeInt32())
            .Activate();

        await AppViewModel.InitializeAsync(isProtocolActivated);

        RegisterUnhandledExceptionHandler();
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
#if DEBUG
            if (Debugger.IsAttached)
                Debugger.Break();
#endif
        };
        TaskScheduler.UnobservedTaskException += (o, e) =>
        {
            e.SetObserved();
            using var scope = AppViewModel.AppServicesScope;
            var logger = scope.ServiceProvider.GetRequiredService<FileLogger>();
            logger.LogError(nameof(TaskScheduler.UnobservedTaskException), e.Exception);
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
#endif
        };
    }
}
