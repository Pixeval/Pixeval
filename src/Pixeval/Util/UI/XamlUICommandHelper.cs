// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.System;
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <param name="isBookmarked"></param>
    /// <param name="isPending">表示<paramref name="isBookmarked"/>的状态即将变化</param>
    public static void RefreshBookmarkCommand(this XamlUICommand command, bool isBookmarked, bool isPending)
    {
        command.Label = command.Description = isBookmarked ? MiscResources.RemoveBookmark : MiscResources.AddBookmark;
        command.IconSource = Symbol.Heart.GetSymbolIconSource(isBookmarked ^ isPending, isBookmarked ? new SolidColorBrush(Colors.Crimson) : null);
    }

    public static void RefreshFollowCommand(this XamlUICommand command, bool isFollowed, bool isPending)
    {
        command.Label = command.Description = isFollowed ? MiscResources.Unfollow : MiscResources.Follow;
        command.IconSource = Symbol.Person.GetSymbolIconSource(isFollowed ^ isPending, isFollowed ? new SolidColorBrush(Colors.Crimson) : null);
    }

    public static XamlUICommand GetNewFollowCommand(bool isFollowed)
    {
        var xamlUiCommand = new XamlUICommand();
        xamlUiCommand.RefreshFollowCommand(isFollowed, false);
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

    public static void RefreshBottomPanelCommand(this XamlUICommand command, bool isExpand)
    {
        command.Label = command.Description = isExpand ? MiscResources.CloseBottomList : MiscResources.OpenBottomList;
        command.IconSource = (isExpand
            ? Symbol.PanelBottomContract
            : Symbol.PanelBottomExpand).GetSymbolIconSource(true);
    }
}
