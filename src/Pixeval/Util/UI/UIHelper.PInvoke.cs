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
    /// <summary>
    /// ReSharper disable once SuspiciousTypeConversion.Global
    /// </summary>
    private static readonly ITaskBarList3 _TaskBarList3Instance = null;// (ITaskBarList3)new TaskBarInstance();

    public static bool TaskBarCustomizationSupported => Environment.OSVersion.Version >= new Version(6, 1);

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
}
