// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Pixeval;
using Pixeval.Themes;

namespace Pixeval.Infrastructure;

/// <summary>
/// 给标准 Avalonia <see cref="Window"/> 套上原生 Windows 11 观感：
/// Mica 背板（整窗）+ 圆角。仅限 Windows 11 且系统开启「透明效果」时生效；
/// 其它平台/条件下为 no-op，窗口保持不透明观感。Pixeval 的窗口都走
/// <see cref="WindowHelper.Init"/> 创建，在 <c>Init</c> 里调用 <see cref="Apply"/>。
/// </summary>
internal static class MicaWindowHelper
{
    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
    private const int DWMWCP_ROUND = 2;
    private const int DWMSBT_TRANSIENTWINDOW = 3; // 亚克力 —— 瞬态表面（菜单/弹出层）上 Mica 不会绘制

    private static bool _acrylicPopupsHooked;
    private static bool _micaConfirmedUnavailable;

    public static void Apply(Window window)
    {
        if (!IsMicaEnabled()) return;
        
        // Avalonia 根据这个 hint 绘制 Mica 背板；透明窗口让它显示出来。
        // 窗口内容面板的半透明表面来自合并的 MicaStyles 字典。
        window.Background = Brushes.Transparent;
        window.TransparencyLevelHint = [WindowTransparencyLevel.Mica];

        window.Opened += (_, _) =>
        {
            // ActualTransparencyLevel 要等平台把 hint 应用到 native 句柄之后才有意义。
            if (window.TryGetPlatformHandle()?.Handle is not { } handle || handle == 0)
                return;

            // None 意味着 DWM 没授予背板：透明窗口会渲染成纯黑（参见UniGetUI #5111）。
            // 锁存 Mica 关闭并回退纯色，让所有表面恢复不透明观感。
            if (window.ActualTransparencyLevel == WindowTransparencyLevel.None)
            {
                NotifyMicaUnavailable();
                if (window.TryFindResource("SolidBackgroundFillColorBaseBrush", window.ActualThemeVariant, out var solidBg)
                    && solidBg is IBrush solidBrush)
                {
                    window.Background = solidBrush;
                }

                return;
            }

            var corner = DWMWCP_ROUND;
            NativeMethods.DwmSetWindowAttribute(handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref corner, sizeof(int));
            window.Background = Brushes.Transparent;
        };
    }

    /// <summary>
    /// 给弹出宿主套 Windows 11 瞬态表面处理。菜单用不透明 WinUI 表面；
    /// 下拉框、ToolTip 和普通 Flyout 用亚克力。启动时注册一次（仅当 Mica 可用时）。
    /// </summary>
    public static void EnableAcrylicPopups()
    {
        if (!IsMicaEnabled() || _acrylicPopupsHooked)
            return;
        _acrylicPopupsHooked = true;

        // 应用内菜单、Flyout、ToolTip、下拉框弹出层都寄宿在 PopupRoot 里。
        Control.LoadedEvent.AddClassHandler<PopupRoot>((root, _) => ApplyAcrylicToPopup(root));

        // 系统托盘右键菜单不是 PopupRoot —— Avalonia 把它放在自己的 Window 里
        //（Avalonia.Win32.TrayIconImpl.TrayPopupRoot），按类型名捕获并套不透明菜单处理。
        Control.LoadedEvent.AddClassHandler<Window>((win, _) =>
        {
            if (win.GetType().Name == "TrayPopupRoot")
                ApplyAcrylicToPopup(win);
        });
    }

    private static void ApplyAcrylicToPopup(TopLevel root)
    {
        if (!IsMicaEnabled())
            return;

        // 菜单用 WinUI 的不透明 flyout 表面；其它瞬态控件（下拉框、ToolTip、普通 Flyout）保留亚克力。
        var isMenu = root.GetType().Name == "TrayPopupRoot"
                     || root.GetVisualDescendants()
                         .Any(control => control is MenuFlyoutPresenter or ContextMenu);
        if (isMenu)
        {
            root.TransparencyLevelHint = [WindowTransparencyLevel.None];
            if (root.TryFindResource("MenuSurfaceBrush", root.ActualThemeVariant, out var resource)
                && resource is IBrush brush)
            {
                root.Background = brush;
            }

            ApplyRoundedCorners(root);
            return;
        }

        // 请求亚克力（而非 Transparent）：Transparent 层是分层窗口，DWM 系统背板不会在上面绘制，
        // 弹出层会完全透明、内容不可读。AcrylicBlur 得到合成窗口，DWM 能真正填充。
        // 此路径仅在 Mica 可用时运行（Win11 + 透明效果），所以亚克力在此总是可用。
        root.TransparencyLevelHint = new[] { WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Blur };
        root.Background = Brushes.Transparent;

        if (root.TryGetPlatformHandle()?.Handle is not { } handle || handle == 0)
            return;

        var corner = DWMWCP_ROUND;
        NativeMethods.DwmSetWindowAttribute(handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref corner, sizeof(int));
        var backdrop = DWMSBT_TRANSIENTWINDOW;
        NativeMethods.DwmSetWindowAttribute(handle, DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int));
    }

    private static void ApplyRoundedCorners(TopLevel root)
    {
        if (root.TryGetPlatformHandle()?.Handle is not { } handle || handle == 0)
            return;

        var corner = DWMWCP_ROUND;
        NativeMethods.DwmSetWindowAttribute(handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref corner, sizeof(int));
    }

    /// <summary>
    /// 何时使用原生 Mica 观感：用户已在设置中开启 Mica、Windows 11+、系统「透明效果」开启，
    /// 且尚无窗口确认背板从未生效；否则保持纯色观感。
    /// 注意：UniGetUI 里软件渲染会导致 Mica 变黑（UniGetUI #5111），此处刻意不做 DXGI GPU
    /// 检测（Pixeval 无此设施）；若在软件渲染环境中出现黑窗，再补回 GPU 检测即可。
    /// </summary>
    public static bool IsMicaEnabled()
        => App.AppViewModel.AppSettings.ApplicationSettings.UseMica
           && OperatingSystem.IsWindows()
           && Environment.OSVersion.Version.Build >= 22000
           && !_micaConfirmedUnavailable
           && IsOsTransparencyEnabled();

    /// <summary>
    /// 一旦某窗口确认 DWM 从未授予背板，锁存 Mica 关闭并移除半透明表面覆写，
    /// 让所有表面回退为纯色观感（防透明窗口渲染成黑窗 —— 参见UniGetUI #5111）。
    /// </summary>
    private static void NotifyMicaUnavailable()
    {
        if (_micaConfirmedUnavailable)
            return;
        _micaConfirmedUnavailable = true;

        if (Application.Current is { } app)
        {
            for (var i = app.Resources.MergedDictionaries.Count - 1; i >= 0; i--)
            {
                if (app.Resources.MergedDictionaries[i] is MicaStyles)
                    app.Resources.MergedDictionaries.RemoveAt(i);
            }
        }
    }

    private static bool IsOsTransparencyEnabled()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        try
        {
            var data = new byte[4];
            var size = data.Length;
            var result = NativeMethods.RegGetValueW(
                NativeMethods.HKEY_CURRENT_USER,
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                "EnableTransparency",
                NativeMethods.RRF_RT_REG_DWORD,
                out _, data, ref size);
            if (result == 0) // ERROR_SUCCESS
                return BitConverter.ToInt32(data, 0) != 0;
        }
        catch { /* 读不到就当透明效果开启 */ }

        return true;
    }

    private static class NativeMethods
    {
        // winreg.h: HKEY_CURRENT_USER = (HKEY)(ULONG_PTR)((LONG)0x80000001) —— x64 上符号扩展。
        public static readonly nint HKEY_CURRENT_USER = new(unchecked((int)0x80000001));
        public const int RRF_RT_REG_DWORD = 0x00000010;

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(nint hwnd, int attr, ref int value, int size);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        public static extern int RegGetValueW(nint hkey, string lpSubKey, string lpValue, int dwFlags,
            out int pdwType, byte[] pvData, ref int pcbData);
    }
}
