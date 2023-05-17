#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/UIHelper.PInvoke.cs
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
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml;
using PInvoke;
using Pixeval.Interop;
using WinRT;
using WinUI3Utilities;

namespace Pixeval.Util.UI;

public static partial class UIHelper
{
    // ReSharper disable once SuspiciousTypeConversion.Global
    private static readonly ITaskBarList3 TaskBarList3Instance = (ITaskBarList3)new TaskBarInstance();

    // see https://github.com/microsoft/Windows-classic-samples/blob/main/Samples/ShareSource/wpf/DataTransferManagerHelper.cs
    private static readonly Guid RiId = new(0xa5caee9b, 0x8708, 0x49d1, 0x8d, 0x36, 0x67, 0xd2, 0x5a, 0x8d, 0xa0, 0x0c);

    public static bool TaskBarCustomizationSupported => Environment.OSVersion.Version >= new Version(6, 1);

    public static IDataTransferManagerInterop DataTransferManagerInterop => DataTransferManager.As<IDataTransferManagerInterop>();

    /// <summary>
    ///     Set the dpi-aware window size, where by "dpi-aware" means that the desired size
    ///     will be multiplied by the scale factor of the monitor which hosts the app
    /// </summary>
    /// <param name="window"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public static void SetWindowSize(this Window window, int width, int height)
    {
        var windowNative = window.As<IWindowNative>();
        var dpi = User32.GetDpiForWindow(windowNative.WindowHandle);
        var scalingFactor = (float)dpi / 96;
        var scaledWidth = (int)(width * scalingFactor);
        var scaledHeight = (int)(height * scalingFactor);

        User32.SetWindowPos(windowNative.WindowHandle, User32.SpecialWindowHandles.HWND_TOP,
            0, 0, scaledWidth, scaledHeight,
            User32.SetWindowPosFlags.SWP_NOMOVE);
    }

    public static void SetTaskBarIconProgressState(TaskBarState state)
    {
        if (TaskBarCustomizationSupported)
        {
            TaskBarList3Instance.SetProgressState((nint)CurrentContext.HWnd, state);
        }
    }

    public static void SetTaskBarIconProgressValue(ulong progressValue, ulong max)
    {
        if (TaskBarCustomizationSupported)
        {
            TaskBarList3Instance.SetProgressValue((nint)CurrentContext.HWnd, progressValue, max);
        }
    }

    // see https://github.com/microsoft/microsoft-ui-xaml/issues/4886
    public static unsafe DataTransferManager GetDataTransferManager()
    {
        var interop = DataTransferManager.As<IDataTransferManagerInterop>();
        var manager = nint.Zero;
        fixed (Guid* id = &RiId)
        {
            interop.GetForWindow((nint) CurrentContext.HWnd, id, (void**) &manager);
            return DataTransferManager.FromAbi(manager);
        }
    }

    public static void ShowShareUI()
    {
        DataTransferManagerInterop.ShowShareUIForWindow((nint) CurrentContext.HWnd);
    }

    /// <summary>
    ///     Get the dpi-aware screen size using win32 API, where by "dpi-aware" means that
    ///     the result will be divided by the scale factor of the monitor that hosts the app
    /// </summary>
    /// <returns>Screen size</returns>
    public static (int, int) GetScreenSize()
    {
        return (User32.GetSystemMetrics(User32.SystemMetric.SM_CXSCREEN), User32.GetSystemMetrics(User32.SystemMetric.SM_CYSCREEN));
    }

    public static nint GetWindowHandle(this Window window)
    {
        return window.As<IWindowNative>().WindowHandle;
    }
}
