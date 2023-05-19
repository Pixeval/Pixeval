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
    private static readonly List<Window> ForkedWindowsInternal = new();

    public static IReadOnlyList<Window> ForkedWindows => ForkedWindowsInternal;

    public static CustomizableWindow Fork(
        AppHelper.InitializeInfo provider,
        Window owner,
        FrameworkElement? titleBar = null,
        RoutedEventHandler? onLoaded = null)
    {
        var w = new CustomizableWindow(owner, onLoaded);
        w.Closed += (_, _) => ForkedWindowsInternal.Remove(w);
        AppHelper.Initialize(provider, w, null, titleBar);
        ForkedWindowsInternal.Add(w);
        return w;
    }
}
