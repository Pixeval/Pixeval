#region Copyright (c) Pixeval/Pixeval.Controls
// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2023 Pixeval.Controls/WindowFactory.cs
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
using System.Linq;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using WinUI3Utilities;
using WinUIEx;

namespace Pixeval.Controls.Windowing;

public static class WindowFactory
{
    public static string IconPath { get; set; } = "";

    public static IWindowSettings WindowSettings { get; set; } = null!;

    public static EnhancedWindow RootWindow { get; private set; } = null!;

    private static readonly Dictionary<ulong, EnhancedWindow> _ForkedWindowsInternal = [];

    public static IReadOnlyDictionary<ulong, EnhancedWindow> ForkedWindows => _ForkedWindowsInternal;

    public static EnhancedWindow GetForkedWindows(ulong key) => _ForkedWindowsInternal[key];

    public static void Initialize(IWindowSettings windowSettings, string iconPath)
    {
        AppHelper.InitializeIsDarkMode();
        WindowSettings = windowSettings;
        Application.Current.RequestedTheme = WindowSettings.Theme switch
        {
            ElementTheme.Light => ApplicationTheme.Light,
            ElementTheme.Dark => ApplicationTheme.Dark,
            _ => Application.Current.RequestedTheme
        };
        IconPath = iconPath;
    }

    public static FrameworkElement GetContentFromHWnd(ulong hWnd)
    {
        return _ForkedWindowsInternal[hWnd].Content.To<FrameworkElement>();
    }

    public static EnhancedWindow GetWindowForElement(UIElement element)
    {
        if (element.XamlRoot is null)
            ThrowHelper.ArgumentNull(element.XamlRoot, $"{nameof(element.XamlRoot)} should not be null.");

        return _ForkedWindowsInternal.Values.FirstOrDefault(window => element.XamlRoot == window.Content.XamlRoot)
               ?? ThrowHelper.ArgumentOutOfRange<UIElement, EnhancedWindow>(element, $"Specified {nameof(element)} is not existed in any of {nameof(ForkedWindows)}.");
    }

    public static EnhancedWindow Create(out EnhancedWindow window)
    {
        RootWindow = window = new EnhancedWindow();
        window.Closed += (sender, _) => _ForkedWindowsInternal.Remove(sender.To<EnhancedWindow>().HWnd);
        _ForkedWindowsInternal[window.HWnd] = window;
        return window;
    }

    public static EnhancedWindow Fork(this EnhancedWindow owner, out ulong hWnd)
    {
        var window = new EnhancedWindow(owner);
        hWnd = window.HWnd;
        window.Closed += (sender, _) => _ForkedWindowsInternal.Remove(sender.To<EnhancedWindow>().HWnd);
        _ForkedWindowsInternal[window.HWnd] = window;
        return window;
    }

    public static EnhancedWindow WithSizeLimit(this EnhancedWindow window, int minWidth = 0, int minHeight = 0, int maxWidth = 0, int maxHeight = 0)
    {
        if (minWidth is not 0)
            window.MinWidth = minWidth;
        if (minHeight is not 0)
            window.MinHeight = minHeight;
        if (maxWidth is not 0)
            window.MaxWidth = maxWidth;
        if (maxHeight is not 0)
            window.MaxHeight = maxHeight;
        return window;
    }

    public static EnhancedWindow WithLoaded(this EnhancedWindow window, RoutedEventHandler onLoaded)
    {
        window.FrameLoaded += onLoaded;
        return window;
    }

    public static EnhancedWindow WithClosing(this EnhancedWindow window, TypedEventHandler<AppWindow, AppWindowClosingEventArgs> onClosed)
    {
        window.AppWindow.Closing += onClosed;
        return window;
    }

    public static EnhancedWindow Init(this EnhancedWindow window, string title, SizeInt32 size = default, bool isMaximized = false)
    {
        window.Initialize(new InitializeInfo
        {
            BackdropType = WindowSettings.Backdrop,
            ExtendTitleBar = true,
            Size = size,
            IconPath = IconPath,
            Title = title,
            IsMaximized = isMaximized,
            Theme = WindowSettings.Theme
        });
        if (!isMaximized)
            window.AppWindow.FullDisplayOnScreen();
        window.FrameLoaded += (_, _) => window.SetTheme(WindowSettings.Theme);
        return window;
    }

    public static void SetBackdrop(BackdropType backdropType)
    {
        foreach (var window in _ForkedWindowsInternal.Values)
            window.SetBackdrop(backdropType);
    }

    public static void SetTheme(ElementTheme theme)
    {
        foreach (var window in _ForkedWindowsInternal.Values)
            window.SetTheme(theme);
    }

    private static void FullDisplayOnScreen(this AppWindow appWindow)
    {
        var hWnd = (nint)appWindow.Id.Value;
        var hWndDesktop = PInvoke.MonitorFromWindow(new HWND(hWnd), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        var info = new MONITORINFO { cbSize = 40 };
        _ = PInvoke.GetMonitorInfo(hWndDesktop, ref info);
        var position = appWindow.Position;
#if DISPLAY
        position.X = info.rcMonitor.Width / 2 + info.rcMonitor.X - appWindow.Size.Width / 2;
        position.Y = info.rcMonitor.Height / 2 + info.rcMonitor.Y - appWindow.Size.Height / 2;
#else
        var left = info.rcMonitor.Width - appWindow.Size.Width;
        if (position.X > left)
            position.X = left;
        var top = info.rcMonitor.Height - appWindow.Size.Height;
        if (position.Y > top)
            position.Y = top;
#endif
        appWindow.Move(position);
    }
}
