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
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using WinUI3Utilities.Controls;

namespace Pixeval.Util.UI;

public static class XamlUiCommandHelper
{
    public static XamlUICommand GetCommand(this string label, IconGlyph icon) =>
        GetCommand(label, icon.GetFontIconSource());

    public static XamlUICommand GetCommand(this string label, IconGlyph icon, VirtualKey key) =>
        GetCommand(label, icon.GetFontIconSource(), key);

    public static XamlUICommand GetCommand(this string label, IconGlyph icon, VirtualKeyModifiers modifiers, VirtualKey key) =>
        GetCommand(label, icon.GetFontIconSource(), modifiers, key);

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

    public static void GetBookmarkCommand(this XamlUICommand command, bool isBookmarked)
    {
        command.Label = command.Description = isBookmarked ? MiscResources.RemoveBookmark : MiscResources.AddBookmark;
        command.IconSource = isBookmarked
            ? IconGlyph.HeartFillEB52.GetFontIconSource(foregroundBrush: new SolidColorBrush(Colors.Crimson))
            : IconGlyph.HeartEB51.GetFontIconSource();
    }

    public static void GetFollowCommand(this XamlUICommand command, bool isFollowed)
    {
        command.Label = command.Description = isFollowed ? MiscResources.Unfollow : MiscResources.Follow;
        command.IconSource = isFollowed
            ? IconGlyph.ContactSolidEA8C.GetFontIconSource(foregroundBrush: new SolidColorBrush(Colors.Crimson))
            : IconGlyph.ContactE77B.GetFontIconSource();
    }

    public static XamlUICommand GetNewFollowCommand(bool isFollowed)
    {
        var xamlUiCommand = new XamlUICommand();
        xamlUiCommand.GetFollowCommand(isFollowed);
        return xamlUiCommand;
    }

    public static XamlUICommand GetNewFollowPrivatelyCommand()
    {
        return MiscResources.FollowPrivately.GetCommand(IconGlyph.FavoriteStarE734);
    }

    public static void GetPlayCommand(this XamlUICommand command, bool isPlaying)
    {
        command.Label = command.Description = isPlaying ? MiscResources.Pause : MiscResources.Play;
        command.IconSource = (isPlaying
            ? IconGlyph.StopE71A
            : IconGlyph.Play36EE4A).GetFontIconSource();
    }

    public static void GetResolutionCommand(this XamlUICommand command, bool isFit)
    {
        command.Label = command.Description = isFit ? MiscResources.RestoreOriginalResolution : MiscResources.UniformToFillResolution;
        command.IconSource = (isFit
            ? IconGlyph.AspectRatioE799
            : IconGlyph.FitPageE9A6).GetFontIconSource();
    }

    public static void GetFullScreenCommand(this XamlUICommand command, bool isFullScreen)
    {
        command.Label = command.Description = isFullScreen ? MiscResources.BackToWindow : MiscResources.FullScreen;
        command.IconSource = (isFullScreen
            ? IconGlyph.BackToWindowE73F
            : IconGlyph.FullScreenE740).GetFontIconSource();
    }
}
