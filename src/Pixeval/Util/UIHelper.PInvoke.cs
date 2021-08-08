using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using PInvoke;
using Pixeval.Interop;
using WinRT;

namespace Pixeval.Util
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

        // ReSharper disable UnusedMember.Global
        public enum TaskBarState
        {
            NoProgress = 0,
            Indeterminate = 1 << 0,
            Normal = 1 << 1,
            Error = 1 << 2,
            Paused = 1 << 3
        }

        [ComImport]
        [Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ITaskBarList3
        {
            [PreserveSig]
            void HrInit();

            [PreserveSig]
            void AddTab(IntPtr hWnd);

            [PreserveSig]
            void DeleteTab(IntPtr hWnd);

            [PreserveSig]
            void ActivateTab(IntPtr hWnd);

            [PreserveSig]
            void SetActiveAlt(IntPtr hWnd);

            [PreserveSig]
            void MarkFullscreenWindow(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

            [PreserveSig]
            void SetProgressValue(IntPtr hWnd, ulong ullCompleted, ulong ullTotal);

            [PreserveSig]
            void SetProgressState(IntPtr hWnd, TaskBarState state);
        }

        [ComImport()]
        [Guid("56fdf344-fd6d-11d0-958a-006097c9a090")]
        [ClassInterface(ClassInterfaceType.None)]
        private class TaskBarInstance
        {
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
    }
}
