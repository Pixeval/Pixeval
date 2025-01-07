#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/UIHelper.PInvoke.cs
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
using Pixeval.Interop;
using Microsoft.UI.Xaml;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;

namespace Pixeval.Util.UI;

public static partial class UiHelper
{
    private const int CLSCTX_INPROC_SERVER = 0x1;

    private static readonly Guid _CTaskBarListId = new Guid("56fdf344-fd6d-11d0-958a-006097c9a090");

    private static readonly ITaskBarList3 _TaskBarList3Instance = GetTaskBarList3();

    private static unsafe ITaskBarList3 GetTaskBarList3()
    {
        fixed (Guid* cls = &_CTaskBarListId)
        {
            var i = typeof(ITaskBarList3).GUID;
            CoCreateInstance((nint)cls, 0, CLSCTX_INPROC_SERVER, (nint)(&i), out var taskBarList);
            var wrappers = new StrategyBasedComWrappers();
            var instance = wrappers.GetOrCreateObjectForComInstance(taskBarList, CreateObjectFlags.UniqueInstance);
            return (ITaskBarList3)instance;
        }
    }

    [LibraryImport("ole32.dll")]
    private static partial int CoCreateInstance(
        nint rclsid,
        nint pUnkOuter,
        uint dwClsContext,
        nint riid,
        out nint ppv);

    public static void SetTaskBarIconProgressState(this Window window, TaskBarState state)
    {
        _TaskBarList3Instance.SetProgressState((nint)window.AppWindow.Id.Value, state);

    }

    public static void SetTaskBarIconProgressValue(this Window window, ulong progressValue, ulong max)
    {
        _TaskBarList3Instance.SetProgressValue((nint)window.AppWindow.Id.Value, progressValue, max);
    }
}
