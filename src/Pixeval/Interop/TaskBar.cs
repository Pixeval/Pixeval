#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/TaskBar.cs
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
using System.Runtime.InteropServices;

namespace Pixeval.Interop;

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