#region Copyright (c) Pixeval/Pixeval.Controls
// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2023 Pixeval.Controls/WindowFactory.cs
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
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Foundation;
using Windows.Graphics;
using WinUI3Utilities;

namespace Pixeval.Controls.Windowing;

public static class WindowFactory
{
    public static string IconAbsolutePath { get; set; } = "";

    public static IWindowSettings WindowSettings { get; set; } = null!;

    public static EnhancedWindow RootWindow => _forkedWindowsInternal[0];

    private static readonly List<EnhancedWindow> _forkedWindowsInternal = [];

    public static IReadOnlyList<EnhancedWindow> ForkedWindows => _forkedWindowsInternal;

    public static void Initialize(IWindowSettings windowSettings, string iconAbsolutePath)
    {
        WindowSettings = windowSettings;
        IconAbsolutePath = iconAbsolutePath;
    }

    public static EnhancedWindow GetWindowForElement(UIElement element)
    {
        if (element.XamlRoot is null)
            ThrowHelper.ArgumentNull(element.XamlRoot, $"{nameof(element.XamlRoot)} should not be null.");

        return _forkedWindowsInternal.Find(window => element.XamlRoot == window.Content.XamlRoot)
               ?? ThrowHelper.ArgumentOutOfRange<UIElement, EnhancedWindow>(element, $"Specified {nameof(element)} is not existed in any of {nameof(ForkedWindows)}.");
    }

    public static EnhancedWindow Create(out EnhancedWindow window)
    {
        window = new EnhancedWindow();
        window.Closed += (sender, _) => _forkedWindowsInternal.Remove(sender.To<EnhancedWindow>());
        _forkedWindowsInternal.Add(window);
        return window;
    }

    public static EnhancedWindow Fork(this EnhancedWindow owner, out EnhancedWindow window)
    {
        window = new EnhancedWindow(owner);
        window.Closed += (sender, _) => _forkedWindowsInternal.Remove(sender.To<EnhancedWindow>());
        _forkedWindowsInternal.Add(window);
        return window;
    }

    public static EnhancedWindow WithSizeLimit(this EnhancedWindow window, int minWidth = 0, int minHeight = 0, int maxWidth = 0, int maxHeight = 0)
    {
        if (minWidth is not 0)
            window.MinWidth = minWidth;
        if (minHeight is not 0)
            window.MinHeight = minHeight;
        if (maxWidth is not 0)
            window.MaxWidth = maxWidth;
        if (maxHeight is not 0)
            window.MaxHeight = maxHeight;
        return window;
    }

    public static EnhancedWindow WithLoaded(this EnhancedWindow window, RoutedEventHandler onLoaded)
    {
        window.FrameLoaded += onLoaded;
        return window;
    }

    public static EnhancedWindow WithClosing(this EnhancedWindow window, TypedEventHandler<AppWindow, AppWindowClosingEventArgs> onClosed)
    {
        window.AppWindow.Closing += onClosed;
        return window;
    }

    public static EnhancedWindow Init(this EnhancedWindow window, string title, SizeInt32 size = default, bool isMaximized = false)
    {
        window.Initialize(new InitializeInfo
        {
            BackdropType = WindowSettings.Backdrop,
            ExtendTitleBar = true,
            Size = size,
            IconPath = IconAbsolutePath,
            Title = title
        });
        if (isMaximized)
            window.AppWindow.Presenter.To<OverlappedPresenter>().Maximize();
        window.FrameLoaded += (s, _) => s.To<FrameworkElement>().RequestedTheme = WindowSettings.Theme;
        return window;
    }

    public static void SetBackdrop(BackdropType backdropType)
    {
        foreach (var window in _forkedWindowsInternal)
            window.SetBackdrop(backdropType);
    }

    public static void SetTheme(ElementTheme theme)
    {
        foreach (var window in _forkedWindowsInternal)
        {
            window.Content.To<FrameworkElement>().RequestedTheme = theme;
        }
    }
}
