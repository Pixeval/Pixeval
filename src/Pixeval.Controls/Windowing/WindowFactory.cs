// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using WinUI3Utilities;

namespace Pixeval.Controls.Windowing;

public static class WindowFactory
{
    public static BitmapImage IconImageSource { get; private set; } = null!;

    public static string IconPath { get; set; } = "";

    public static IWindowSettings WindowSettings { get; set; } = null!;

    public static EnhancedWindow RootWindow { get; private set; } = null!;

    private static readonly Dictionary<ulong, EnhancedWindow> _ForkedWindowsInternal = [];

    /// <summary>
    /// 所有窗口，包括主窗口
    /// </summary>
    public static IReadOnlyDictionary<ulong, EnhancedWindow> ForkedWindows => _ForkedWindowsInternal;

    public static EnhancedWindow GetForkedWindows(ulong key) => _ForkedWindowsInternal[key];

    public static void Initialize(IWindowSettings windowSettings, string iconPath, string svgIconPath)
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
        IconImageSource = new(new($"ms-appx:///{svgIconPath}"));
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

    public static EnhancedWindow Create(EnhancedPage content)
    {
        RootWindow = new EnhancedWindow(content);
        RootWindow.Closed += (sender, _) => _ForkedWindowsInternal.Remove(sender.To<EnhancedWindow>().HWnd);
        _ForkedWindowsInternal[RootWindow.HWnd] = RootWindow;
        return RootWindow;
    }

    public static EnhancedWindow Fork(this EnhancedWindow owner, EnhancedPage content, out ulong hWnd)
    {
        var window = new EnhancedWindow(owner, content);
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

    public static EnhancedWindow WithInitialized(this EnhancedWindow window, RoutedEventHandler onLoaded)
    {
        window.Initialized += onLoaded;
        return window;
    }

    /// <summary>
    /// 用户手动关闭时响应，代码关闭不触发<see cref="AppWindow.Closing"/>事件
    /// </summary>
    /// <param name="window"></param>
    /// <param name="onClosed"></param>
    /// <returns></returns>
    public static EnhancedWindow WithClosing(this EnhancedWindow window, TypedEventHandler<AppWindow, AppWindowClosingEventArgs>? onClosed)
    {
        if (onClosed is not null)
            window.AppWindow.Closing += onClosed;
        return window;
    }

    /// <summary>
    /// 窗口关闭时一定触发
    /// </summary>
    /// <param name="window"></param>
    /// <param name="onDestroying"></param>
    /// <returns></returns>
    public static EnhancedWindow WithDestroying(this EnhancedWindow window, TypedEventHandler<AppWindow, object> onDestroying)
    {
        window.AppWindow.Destroying += onDestroying;
        return window;
    }

    public static EnhancedWindow Init(this EnhancedWindow window, string title, SizeInt32 size = default, bool isMaximized = false)
    {
        window.Initialize(new InitializeInfo
        {
            BackdropType = WindowSettings.Backdrop,
            ExtendTitleBar = true,
            IconPath = IconPath,
            Title = title,
            IsMaximized = isMaximized,
            Theme = WindowSettings.Theme
        });
        if (!isMaximized)
            window.AppWindow.FullDisplayOnScreen(size);
        window.Initialized += (_, _) => window.SetTheme(WindowSettings.Theme);
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

    private static void FullDisplayOnScreen(this AppWindow appWindow, SizeInt32 desiredSize)
    {
        var hWnd = (nint) appWindow.Id.Value;
        var hWndDesktop = PInvoke.MonitorFromWindow(new HWND(hWnd), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        var info = new MONITORINFO { cbSize = 40 };
        _ = PInvoke.GetMonitorInfo(hWndDesktop, ref info);
        var position = appWindow.Position;
        var size = desiredSize == default ? appWindow.Size : desiredSize;
        var left = info.rcWork.Width - size.Width;
        if (left < 0)
        {
            left = 0;
            desiredSize.Width = info.rcWork.Width;
        }
        if (position.X > left)
            position.X = left;
        var top = info.rcWork.Height - appWindow.Size.Height;
        if (top < 0)
        {
            top = 0;
            desiredSize.Height = info.rcWork.Height;
        }
        if (position.Y > top)
            position.Y = top;
        appWindow.MoveAndResize(new RectInt32(position.X, position.Y, desiredSize.Width, desiredSize.Height));
    }
}
