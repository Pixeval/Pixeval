// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Pixeval.AppManagement;

namespace Pixeval.Utilities;

public static class WindowHelper
{
    /// <param name="window"></param>
    extension(Window window)
    {
        public Window Init(string? title, Uri iconPath, double width = 0, double height = 0, double minWidth = 0, double minHeight = 0, bool isMaximized = false)
        {
            return window.Init(title, new WindowIcon(AssetLoader.Open(iconPath)), width, height, minWidth, minHeight, isMaximized);
        }

        public Window Init(string? title, WindowIcon? icon, double width = 0, double height = 0, double minWidth = 0, double minHeight = 0, bool isMaximized = false)
        {
            window.ExtendClientAreaToDecorationsHint = true;
            window.WindowDecorations = WindowDecorations.Full;
            window.Icon = icon;
            window.Title = title;
            window.WindowState = isMaximized ? WindowState.Maximized : WindowState.Normal;
            window.Width = width;
            window.Height = height;
            window.MinWidth = minWidth;
            window.MinHeight = minHeight;
            window.ClosingBehavior = WindowClosingBehavior.OwnerAndChildWindows;
            window.Closing += (o, _) =>
            {
                if (o is Window w)
                    AppInfo.SaveWindowContext(w);
            };
            return window;
        }

        public Window Fork(Window source)
        {
            return window.Init(source.Title, source.Icon, source.Width, source.Height, source.MinWidth, source.MinHeight);
        }
    }
}
