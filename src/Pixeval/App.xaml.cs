#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/App.xaml.cs
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
using System.Linq;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.AppLifecycle;
using Pixeval.Activation;
using Pixeval.AppManagement;
using Pixeval.Interop;
using Pixeval.Util.UI;
using WinRT;
using AppContext = Pixeval.AppManagement.AppContext;
using ApplicationTheme = Pixeval.Options.ApplicationTheme;

namespace Pixeval;

public partial class App
{
    private const string ApplicationWideFontKey = "ContentControlThemeFontFamily";

    private static MicaController? _backdropController;

    private static WindowsSystemDispatcherQueueHelper? _dispatcherQueueHelper;

    private static SystemBackdropConfiguration? _systemBackdropConfiguration;

    public App()
    {
        // The theme can only be changed in ctor
        AppViewModel = new AppViewModel(this) { AppSetting = AppContext.LoadConfiguration() ?? AppSetting.CreateDefault() };
        RequestedTheme = AppViewModel.AppSetting.Theme switch
        {
            ApplicationTheme.Dark => Microsoft.UI.Xaml.ApplicationTheme.Dark,
            ApplicationTheme.Light => Microsoft.UI.Xaml.ApplicationTheme.Light,
            _ => RequestedTheme
        };
        AppInstance.GetCurrent().Activated += (_, arguments) => ActivationRegistrar.Dispatch(arguments);
        InitializeComponent();
    }

    public static AppViewModel AppViewModel { get; private set; } = null!;

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        if (AppInstance.GetCurrent().GetActivatedEventArgs().Kind == ExtendedActivationKind.ToastNotification)
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
        await AppViewModel.InitializeAsync(isProtocolActivated);
    }

    /// <summary>
    ///     Are we Windows 11 or not?
    /// </summary>
    public static bool IsWindows11()
    {
        // Windows 11 starts with 10.0.22000
        return Environment.OSVersion.Version.Build >= 22000;
    }

    /// <summary>
    ///     Calculate the window size by current resolution
    /// </summary>
    public static (int, int) PredetermineEstimatedWindowSize()
    {
        return UIHelper.GetScreenSize() switch
        {
            // 这 就 是 C #
            ( >= 2560, >= 1440) => (1600, 900),
            ( > 1600, > 900) => (1280, 720),
            _ => (800, 600)
        };
    }

    public static void TryApplyMica()
    {
        if (MicaController.IsSupported())
        {
            _dispatcherQueueHelper = new WindowsSystemDispatcherQueueHelper();
            _dispatcherQueueHelper.EnsureWindowsSystemDispatcherQueueController();

            _systemBackdropConfiguration = new SystemBackdropConfiguration();
            AppViewModel.Window.Activated += WindowOnActivated;
            AppViewModel.Window.Closed += WindowOnClosed;
            ((FrameworkElement) AppViewModel.Window.Content).ActualThemeChanged += OnActualThemeChanged;

            _systemBackdropConfiguration.IsInputActive = true;
            SetConfigurationSourceTheme();

            _backdropController = new MicaController();

            _backdropController.AddSystemBackdropTarget(AppViewModel.Window.As<ICompositionSupportsSystemBackdrop>());
            _backdropController.SetSystemBackdropConfiguration(_systemBackdropConfiguration);
        }
    }

    private static void OnActualThemeChanged(FrameworkElement sender, object args)
    {
        if (_systemBackdropConfiguration != null)
        {
            SetConfigurationSourceTheme();
        }
    }

    private static void SetConfigurationSourceTheme()
    {
        _systemBackdropConfiguration!.Theme = AppViewModel.Window.Content switch
        {
            FrameworkElement { ActualTheme: ElementTheme.Dark } => SystemBackdropTheme.Dark,
            FrameworkElement { ActualTheme: ElementTheme.Light } => SystemBackdropTheme.Light,
            FrameworkElement { ActualTheme: ElementTheme.Default } => SystemBackdropTheme.Default,
            _ => _systemBackdropConfiguration!.Theme
        };
    }

    private static void WindowOnClosed(object sender, WindowEventArgs args)
    {
        if (_backdropController != null)
        {
            _backdropController.Dispose();
            _backdropController = null;
        }

        AppViewModel.Window.Activated -= WindowOnActivated;
        _systemBackdropConfiguration = null;
    }

    private static void WindowOnActivated(object sender, WindowActivatedEventArgs args)
    {
        _systemBackdropConfiguration!.IsInputActive = args.WindowActivationState is not WindowActivationState.Deactivated;
    }
}