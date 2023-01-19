#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/UIHelper.App.cs
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
using Windows.Foundation;
using Microsoft.UI.Xaml.Media.Animation;
using PInvoke;
using WinUI3Utilities;

namespace Pixeval.Util.UI;

public partial class UIHelper
{
    public static void RootFrameNavigate(Type type, object parameter, NavigationTransitionInfo infoOverride)
    {
        App.AppViewModel.AppWindowRootFrame.Navigate(type, parameter, infoOverride);
    }

    public static void RootFrameNavigate(Type type, object parameter)
    {
        App.AppViewModel.AppWindowRootFrame.Navigate(type, parameter);
    }

    public static void RootFrameNavigate(Type type)
    {
        App.AppViewModel.AppWindowRootFrame.Navigate(type);
    }

    public static (int, int) GetAppWindowSizeTuple()
    {
        var windowSize = CurrentContext.AppWindow.Size;
        return (windowSize.Width, windowSize.Height);
    }

    public static Size GetAppWindowSize()
    {
        return CurrentContext.AppWindow.Size.ToWinRtSize();
    }

    public static Size GetDpiAwareAppWindowSize()
    {
        var dpi = User32.GetDpiForWindow(CurrentContext.HWnd);
        var size = GetAppWindowSize();
        var scalingFactor = (float) dpi / 96;
        return new Size(size.Width / scalingFactor, size.Height / scalingFactor);
    }
}