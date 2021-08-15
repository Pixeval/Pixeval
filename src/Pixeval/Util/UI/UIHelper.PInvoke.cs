using System;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml;
using PInvoke;
using Pixeval.Interop;
using WinRT;

namespace Pixeval.Util.UI
{
    public static partial class UIHelper
    {
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
                TaskBarList3Instance.SetProgressState(App.GetMainWindowHandle(), state);
            }
        }

        public static void SetTaskBarIconProgressValue(ulong progressValue, ulong max)
        {
            if (TaskBarCustomizationSupported)
            {
                TaskBarList3Instance.SetProgressValue(App.GetMainWindowHandle(), progressValue, max);
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
                interop.GetForWindow(App.GetMainWindowHandle(), id, (void**) &manager);
                return DataTransferManager.FromAbi(manager);
            }
        }

        public static void ShowShareUI()
        {
            DataTransferManagerInterop.ShowShareUIForWindow(App.GetMainWindowHandle());
        }
    }
}
