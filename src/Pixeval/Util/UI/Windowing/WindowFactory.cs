#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/WindowManager.cs
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

using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Pixeval.Options;
using Windows.Foundation;
using Windows.Graphics;
using Windows.UI;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Media;
using PInvoke;
using WinUI3Utilities;
using AppTheme = Pixeval.Options.ApplicationTheme;
using ApplicationTheme = Microsoft.UI.Xaml.ApplicationTheme;

namespace Pixeval.Util.UI.Windowing;

public static class WindowFactory
{
    public static EnhancedWindow RootWindow => ForkedWindowsInternal[0];

    private static readonly List<EnhancedWindow> ForkedWindowsInternal = new();

    public static IReadOnlyList<EnhancedWindow> ForkedWindows => ForkedWindowsInternal;

    public static EnhancedWindow Create(out EnhancedWindow window)
    {
        var w = window = new();
        if (ForkedWindowsInternal.Count is 0)
            CurrentContext.Window = window;
        window.Closed += (_, _) => ForkedWindowsInternal.Remove(w);
        ForkedWindowsInternal.Add(window);
        return window;
    }

    public static EnhancedWindow Fork(this EnhancedWindow owner, out EnhancedWindow window)
    {
        var w = window = new(owner);
        window.Closed += (_, _) => ForkedWindowsInternal.Remove(w);
        ForkedWindowsInternal.Add(window);
        return window;
    }

    public static EnhancedWindow WithLoaded(this EnhancedWindow window, RoutedEventHandler onLoaded)
    {
        window.FrameLoaded += onLoaded;
        return window;
    }

    public static EnhancedWindow WithClosed(this EnhancedWindow window, TypedEventHandler<object, WindowEventArgs> onClosed)
    {
        window.Closed += onClosed;
        return window;
    }

    public static EnhancedWindow Init(this EnhancedWindow window, SizeInt32 size = default)
    {
        window.Initialize(new()
        {
            BackdropType = App.AppViewModel.AppSetting.AppBackdrop switch
            {
                ApplicationBackdropType.None => BackdropType.None,
                ApplicationBackdropType.Acrylic => BackdropType.Acrylic,
                ApplicationBackdropType.Mica => BackdropType.Mica,
                ApplicationBackdropType.MicaAlt => BackdropType.MicaAlt,
                _ => WinUI3Utilities.ThrowHelper.ArgumentOutOfRange<ApplicationBackdropType, BackdropType>(App.AppViewModel.AppSetting.AppBackdrop)
            },
            TitleBarType = TitleBarHelper.TitleBarType.AppWindow,
            Size = size
        });
        var theme = GetElementTheme(App.AppViewModel.AppSetting.Theme);
        SetAppWindowTitleBarButtonColor(window, theme);
        window.FrameLoaded += (s, _) =>
        {
            s.To<FrameworkElement>().RequestedTheme = theme;
        };
        return window;
    }

    public static void SetBackdrop(ApplicationBackdropType backdropType)
    {
        foreach (var window in ForkedWindowsInternal)
        {
            window.SystemBackdrop = backdropType switch
            {
                ApplicationBackdropType.None => null,
                ApplicationBackdropType.Acrylic => new DesktopAcrylicBackdrop(),
                ApplicationBackdropType.Mica => new MicaBackdrop(),
                ApplicationBackdropType.MicaAlt => new MicaBackdrop { Kind = MicaKind.BaseAlt },
                _ => WinUI3Utilities.ThrowHelper.ArgumentOutOfRange<ApplicationBackdropType, SystemBackdrop>(backdropType)
            };
        }
    }

    public static void SetTheme(AppTheme theme)
    {
        var t = GetElementTheme(theme);

        foreach (var window in ForkedWindowsInternal)
        {
            window.Content.To<FrameworkElement>().RequestedTheme = t;
            SetAppWindowTitleBarButtonColor(window, t);
        }
    }

    private static ElementTheme GetElementTheme(AppTheme theme)
    {
        return theme switch
        {
            AppTheme.Dark => ElementTheme.Dark,
            AppTheme.Light => ElementTheme.Light,
            AppTheme.SystemDefault => Application.Current.RequestedTheme switch
            {
                ApplicationTheme.Light => ElementTheme.Light,
                ApplicationTheme.Dark => ElementTheme.Dark,
                _ => WinUI3Utilities.ThrowHelper.ArgumentOutOfRange<ApplicationTheme, ElementTheme>(Application.Current.RequestedTheme)
            },
            _ => WinUI3Utilities.ThrowHelper.ArgumentOutOfRange<AppTheme, ElementTheme>(theme)
        };
    }

    /// <summary>
    /// Work when in <see cref="TitleBarHelper.TitleBarType.AppWindow"/>
    /// </summary>
    /// <param name="window"></param>
    /// <param name="theme"></param>
    private static void SetAppWindowTitleBarButtonColor(Window window, ElementTheme theme)
    {
        window.AppWindow.TitleBar.ButtonForegroundColor = theme switch
        {
            ElementTheme.Light => Colors.Black,
            ElementTheme.Dark => Colors.White,
            _ => WinUI3Utilities.ThrowHelper.ArgumentOutOfRange<ElementTheme, Color>(theme)
        };
        window.AppWindow.TitleBar.ButtonHoverBackgroundColor = theme switch
        {
            ElementTheme.Light => new() { A = 0x33, R = 0, G = 0, B = 0 },
            ElementTheme.Dark => new() { A = 0x33, R = 0xFF, G = 0xFF, B = 0xFF },
            _ => WinUI3Utilities.ThrowHelper.ArgumentOutOfRange<ElementTheme, Color>(theme)
        };
    }

    #region Window

    /// <summary>
    /// Work when in <see cref="TitleBarHelper.TitleBarType.Window"/>
    /// </summary>
    /// <param name="window"></param>
    /// <param name="theme"></param>
    private static void SetWindowTitleBarButtonColor(Window window, ElementTheme theme)
    {
        Application.Current.Resources["WindowCaptionForeground"] = theme switch
        {
            ElementTheme.Light => Colors.Black,
            ElementTheme.Dark => Colors.White,
            _ => WinUI3Utilities.ThrowHelper.ArgumentOutOfRange<ElementTheme, Color>(theme)
        };

        TriggerTitleBarRepaint(window);
    }

    private static void TriggerTitleBarRepaint(Window window)
    {
        // to trigger repaint tracking task id 38044406
        var hWnd = (nint)window.AppWindow.Id.Value;
        var activeWindow = User32.GetActiveWindow();
        if (hWnd == activeWindow)
        {
            _ = User32.SendMessage(hWnd, User32.WindowMessage.WM_ACTIVATE, 0, 0);
            _ = User32.SendMessage(hWnd, User32.WindowMessage.WM_ACTIVATE, 1, 0);
        }
        else
        {
            _ = User32.SendMessage(hWnd, User32.WindowMessage.WM_ACTIVATE, 1, 0);
            _ = User32.SendMessage(hWnd, User32.WindowMessage.WM_ACTIVATE, 0, 0);
        }
    }

    #endregion
}
