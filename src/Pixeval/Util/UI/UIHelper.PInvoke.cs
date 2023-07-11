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
using Microsoft.UI.Xaml;
using Pixeval.Interop;
using Windows.ApplicationModel.DataTransfer;

namespace Pixeval.Util.UI;

public static partial class UIHelper
{
    // ReSharper disable once SuspiciousTypeConversion.Global
    private static readonly ITaskBarList3 TaskBarList3Instance = (ITaskBarList3)new TaskBarInstance();

    // see https://github.com/microsoft/Windows-classic-samples/blob/main/Samples/ShareSource/wpf/DataTransferManagerHelper.cs
    private static readonly Guid RiId = new(0xa5caee9b, 0x8708, 0x49d1, 0x8d, 0x36, 0x67, 0xd2, 0x5a, 0x8d, 0xa0, 0x0c);

    public static bool TaskBarCustomizationSupported => Environment.OSVersion.Version >= new Version(6, 1);

    public static IDataTransferManagerInterop DataTransferManagerInterop => DataTransferManager.As<IDataTransferManagerInterop>();

    public static void SetTaskBarIconProgressState(this Window window, TaskBarState state)
    {
        if (TaskBarCustomizationSupported)
        {
            TaskBarList3Instance.SetProgressState((nint)window.AppWindow.Id.Value, state);
        }
    }

    public static void SetTaskBarIconProgressValue(this Window window, ulong progressValue, ulong max)
    {
        if (TaskBarCustomizationSupported)
        {
            TaskBarList3Instance.SetProgressValue((nint)window.AppWindow.Id.Value, progressValue, max);
        }
    }

    // see https://github.com/microsoft/microsoft-ui-xaml/issues/4886
    public static unsafe DataTransferManager GetDataTransferManager(this Window window)
    {
        var interop = DataTransferManager.As<IDataTransferManagerInterop>();
        var manager = nint.Zero;
        fixed (Guid* id = &RiId)
        {
            interop.GetForWindow((nint)window.AppWindow.Id.Value, id, (void**)&manager);
            return DataTransferManager.FromAbi(manager);
        }
    }

    public static void ShowShareUI(this Window window)
    {
        DataTransferManagerInterop.ShowShareUIForWindow((nint)window.AppWindow.Id.Value);
    }
}
