#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/WindowManager.cs
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

using System.Collections.Generic;
using Microsoft.UI.Xaml;
using WinUI3Utilities;

namespace Pixeval.Util.UI.Windowing;

public static class WindowFactory
{
    private static readonly List<CustomizableWindow> ForkedWindowsInternal = new();

    public static IReadOnlyList<CustomizableWindow> ForkedWindows => ForkedWindowsInternal;

    public static CustomizableWindow Fork(this Window owner, out CustomizableWindow window)
    {
        var w = window = new(owner);
        window.Closed += (_, _) =>
        {
            w.Close();
            ForkedWindowsInternal.Remove(w);
        };
        ForkedWindowsInternal.Add(window);
        return window;
    }

    public static CustomizableWindow WithLoaded(this CustomizableWindow window, RoutedEventHandler onLoaded)
    {
        window.FrameLoaded += onLoaded;
        return window;
    }

    public static CustomizableWindow Initialize(this CustomizableWindow window, WindowHelper.InitializeInfo provider)
    {
        WindowHelper.Initialize(window, provider);
        return window;
    }
}
