using System;
using System.Runtime.InteropServices;

namespace Pixeval.Interop
{
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
    public interface ITaskBarList3
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

    [ComImport]
    [Guid("56fdf344-fd6d-11d0-958a-006097c9a090")]
    [ClassInterface(ClassInterfaceType.None)]
    public class TaskBarInstance
    {
    }
}
