#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/XamlUICommandHelper.cs
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

using Windows.System;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Symbol = FluentIcons.Common.Symbol;

namespace Pixeval.Util.UI;

public static class XamlUiCommandHelper
{
    public static XamlUICommand GetCommand(this string label, Symbol icon) =>
        GetCommand(label, icon.GetSymbolIconSource());

    public static XamlUICommand GetCommand(this string label, Symbol icon, VirtualKey key) =>
        GetCommand(label, icon.GetSymbolIconSource(), key);

    public static XamlUICommand GetCommand(this string label, Symbol icon, VirtualKeyModifiers modifiers, VirtualKey key) =>
        GetCommand(label, icon.GetSymbolIconSource(), modifiers, key);

    public static XamlUICommand GetCommand(this string label, IconSource icon) =>
        new()
        {
            Label = label,
            Description = label,
            IconSource = icon
        };

    public static XamlUICommand GetCommand(this string label, IconSource icon, VirtualKey key) =>
        GetCommand(label, icon, VirtualKeyModifiers.None, key);

    public static XamlUICommand GetCommand(this string label, IconSource icon, VirtualKeyModifiers modifiers, VirtualKey key) =>
        new()
        {
            Label = label,
            IconSource = icon,
            KeyboardAccelerators = { new KeyboardAccelerator { Modifiers = modifiers, Key = key } }
        };

    public static void RefreshBookmarkCommand(this XamlUICommand command, bool isBookmarked)
    {
        command.Label = command.Description = isBookmarked ? MiscResources.RemoveBookmark : MiscResources.AddBookmark;
        command.IconSource = Symbol.Heart.GetSymbolIconSource(isBookmarked, isBookmarked ? new SolidColorBrush(Colors.Crimson) : null);
    }

    public static void RefreshFollowCommand(this XamlUICommand command, bool isFollowed)
    {
        command.Label = command.Description = isFollowed ? MiscResources.Unfollow : MiscResources.Follow;
        command.IconSource = Symbol.Person.GetSymbolIconSource(isFollowed, isFollowed ? new SolidColorBrush(Colors.Crimson) : null);
    }

    public static XamlUICommand GetNewFollowCommand(bool isFollowed)
    {
        var xamlUiCommand = new XamlUICommand();
        xamlUiCommand.RefreshFollowCommand(isFollowed);
        return xamlUiCommand;
    }

    public static XamlUICommand GetNewFollowPrivatelyCommand()
    {
        return MiscResources.FollowPrivately.GetCommand(Symbol.Star);
    }

    public static void RefreshPlayCommand(this XamlUICommand command, bool isPlaying)
    {
        command.Label = command.Description = isPlaying ? MiscResources.Pause : MiscResources.Play;
        command.IconSource = (isPlaying
            ? Symbol.Stop
            : Symbol.Play).GetSymbolIconSource();
    }

    public static void RefreshResolutionCommand(this XamlUICommand command, bool isFit)
    {
        command.Label = command.Description = isFit ? MiscResources.RestoreOriginalResolution : MiscResources.UniformToFillResolution;
        command.IconSource = (isFit
            ? Symbol.RatioOneToOne
            : Symbol.PageFit).GetSymbolIconSource();
    }

    public static void RefreshFullScreenCommand(this XamlUICommand command, bool isFullScreen)
    {
        command.Label = command.Description = isFullScreen ? MiscResources.BackToWindow : MiscResources.FullScreen;
        command.IconSource = (isFullScreen
            ? Symbol.ArrowMinimize
            : Symbol.ArrowMaximize).GetSymbolIconSource();
    }
}
