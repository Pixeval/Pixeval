using System;
using System.Runtime.InteropServices;

namespace Pixeval.Interop
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3A3DCD6C-3EAB-43DC-BCDE-45671CE800C8")]
    public interface IDataTransferManagerInterop
    {
        unsafe void GetForWindow(IntPtr appWindow, Guid* riId, [Optional] void** dataTransferManager);

        void ShowShareUIForWindow(IntPtr appWindow);
    }
}