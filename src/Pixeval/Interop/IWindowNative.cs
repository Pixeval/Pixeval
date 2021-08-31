using System;
using System.Runtime.InteropServices;

namespace Pixeval.Interop //任意保留一种方法
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
    internal interface IWindowNative
    {
        IntPtr WindowHandle { get; }
    }

    public static class GetWindow
    {
	    public static IntPtr HWnd => WinRT.Interop.WindowNative.GetWindowHandle(App.Window);
    }
}