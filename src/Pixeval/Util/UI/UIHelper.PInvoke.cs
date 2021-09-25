using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using PInvoke;
using Pixeval.Interop;
using WinRT;

namespace Pixeval.Util.UI
{
    public static partial class UIHelper
    {
        /// <summary>
        /// Set the dpi-aware window size, where by "dpi-aware" means that the desired size
        /// will be multiplied by the scale factor of the monitor which hosts the app
        /// </summary>
        /// <param name="window"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void SetWindowSize(this Window window, int width, int height)
        {
            var windowNative = window.As<IWindowNative>(); // see https://github.com/microsoft/WinUI-3-Demos/blob/master/src/Build2020Demo/DemoBuildCs/DemoBuildCs/DemoBuildCs/App.xaml.cs
            var dpi = User32.GetDpiForWindow(windowNative.WindowHandle);
            var scalingFactor = (float) dpi / 96;
            var scaledWidth = (int) (width * scalingFactor);
            var scaledHeight = (int) (height * scalingFactor);

            User32.SetWindowPos(windowNative.WindowHandle, User32.SpecialWindowHandles.HWND_TOP,
                0, 0, scaledWidth, scaledHeight,
                User32.SetWindowPosFlags.SWP_NOMOVE);
        }

        public static bool TaskBarCustomizationSupported => Environment.OSVersion.Version >= new Version(6, 1);

        // ReSharper disable once SuspiciousTypeConversion.Global
        private static readonly ITaskBarList3 TaskBarList3Instance = (ITaskBarList3) new TaskBarInstance();

        public static void SetTaskBarIconProgressState(TaskBarState state)
        {
            if (TaskBarCustomizationSupported)
            {
                TaskBarList3Instance.SetProgressState(App.AppViewModel.GetMainWindowHandle(), state);
            }
        }

        public static void SetTaskBarIconProgressValue(ulong progressValue, ulong max)
        {
            if (TaskBarCustomizationSupported)
            {
                TaskBarList3Instance.SetProgressValue(App.AppViewModel.GetMainWindowHandle(), progressValue, max);
            }
        }

        // see https://github.com/microsoft/Windows-classic-samples/blob/main/Samples/ShareSource/wpf/DataTransferManagerHelper.cs
        private static readonly Guid RiId = new(0xa5caee9b, 0x8708, 0x49d1, 0x8d, 0x36, 0x67, 0xd2, 0x5a, 0x8d, 0xa0, 0x0c);

        public static IDataTransferManagerInterop DataTransferManagerInterop => DataTransferManager.As<IDataTransferManagerInterop>();

        // see https://github.com/microsoft/microsoft-ui-xaml/issues/4886
        public static unsafe DataTransferManager GetDataTransferManager()
        {
            var interop = DataTransferManager.As<IDataTransferManagerInterop>();
            var manager = IntPtr.Zero;
            fixed (Guid* id = &RiId)
            {
                interop.GetForWindow(App.AppViewModel.GetMainWindowHandle(), id, (void**) &manager);
                return DataTransferManager.FromAbi(manager);
            }
        }

        public static void ShowShareUI()
        {
            DataTransferManagerInterop.ShowShareUIForWindow(App.AppViewModel.GetMainWindowHandle());
        }

        /// <summary>
        /// Get the dpi-aware screen size using win32 API, where by "dpi-aware" means that
        /// the result will be divided by the scale factor of the monitor that hosts the app
        /// </summary>
        /// <returns>Screen size</returns>
        public static (int, int) GetScreenSize()
        {
            return (User32.GetSystemMetrics(User32.SystemMetric.SM_CXSCREEN), User32.GetSystemMetrics(User32.SystemMetric.SM_CYSCREEN));
        }

        public static IntPtr GetWindowHandle(this Window window)
        {
            return window.As<IWindowNative>().WindowHandle;
        }

        public static (int, int) GetWindowSizeTuple(IntPtr hWnd)
        {
            User32.GetWindowRect(hWnd, out var rect);
            var dpi = User32.GetDpiForWindow(hWnd) / 96d;
            return ((int, int)) (rect.right / dpi - rect.left / dpi, rect.bottom / dpi - rect.top / dpi);
        }

        public static Size GetWindowSize(IntPtr hWnd)
        {
            var (width, height) = GetWindowSizeTuple(hWnd);
            return new Size(width, height);
        }

        public static (int, int) SizeTuple(this Window window)
        {
            var hWnd = window.GetWindowHandle();
            return GetWindowSizeTuple(hWnd);
        }

        public static Size Size(this Window window)
        {
            var hWnd = window.GetWindowHandle();
            return GetWindowSize(hWnd);
        }
    }
}
