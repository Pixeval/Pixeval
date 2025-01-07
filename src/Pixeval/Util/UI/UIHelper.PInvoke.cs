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
using Windows.ApplicationModel.DataTransfer;
using Pixeval.Controls.Windowing;
using Pixeval.Interop;

namespace Pixeval.Util.UI;

public static partial class UiHelper
{
    // ReSharper disable once SuspiciousTypeConversion.Global
    private static readonly ITaskBarList3 _TaskBarList3Instance = (ITaskBarList3)new TaskBarInstance();

    // see https://github.com/microsoft/Windows-classic-samples/blob/main/Samples/ShareSource/wpf/DataTransferManagerHelper.cs
    private static readonly Guid _RiId = new(0xa5caee9b, 0x8708, 0x49d1, 0x8d, 0x36, 0x67, 0xd2, 0x5a, 0x8d, 0xa0, 0x0c);

    public static bool TaskBarCustomizationSupported => Environment.OSVersion.Version >= new Version(6, 1);

    public static IDataTransferManagerInterop DataTransferManagerInterop => DataTransferManager.As<IDataTransferManagerInterop>();

    public static void SetTaskBarIconProgressState(this EnhancedWindow window, TaskBarState state)
    {
        if (TaskBarCustomizationSupported)
        {
            _TaskBarList3Instance.SetProgressState((nint)window.HWnd, state);
        }
    }

    public static void SetTaskBarIconProgressValue(this EnhancedWindow window, ulong progressValue, ulong max)
    {
        if (TaskBarCustomizationSupported)
        {
            _TaskBarList3Instance.SetProgressValue((nint)window.HWnd, progressValue, max);
        }
    }

    // see https://github.com/microsoft/microsoft-ui-xaml/issues/4886
    public static unsafe DataTransferManager GetDataTransferManager(this ulong hWnd)
    {
        var interop = DataTransferManager.As<IDataTransferManagerInterop>();
        var manager = nint.Zero;
        fixed (Guid* id = &_RiId)
        {
            interop.GetForWindow((nint)hWnd, id, (void**)&manager);
            return DataTransferManager.FromAbi(manager);
        }
    }

    public static void ShowShareUi(this ulong hWnd)
    {
        DataTransferManagerInterop.ShowShareUIForWindow((nint)hWnd);
    }
}
