// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Pixeval.Interop;

public enum TaskBarState
{
    NoProgress = 0,
    Indeterminate = 1 << 0,
    Normal = 1 << 1,
    Error = 1 << 2,
    Paused = 1 << 3
}

[GeneratedComInterface]
[Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
public partial interface ITaskBarList3
{
    [PreserveSig]
    void HrInit();

    [PreserveSig]
    void AddTab(nint hWnd);

    [PreserveSig]
    void DeleteTab(nint hWnd);

    [PreserveSig]
    void ActivateTab(nint hWnd);

    [PreserveSig]
    void SetActiveAlt(nint hWnd);

    [PreserveSig]
    void MarkFullscreenWindow(nint hWnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

    [PreserveSig]
    void SetProgressValue(nint hWnd, ulong ullCompleted, ulong ullTotal);

    [PreserveSig]
    void SetProgressState(nint hWnd, TaskBarState state);
}
