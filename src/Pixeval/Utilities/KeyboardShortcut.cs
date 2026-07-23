// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Input;

namespace Pixeval.Utilities;

/// <summary>
/// Matches and executes shortcuts from bubbling key handlers after focused descendants have had a chance to handle the key.
/// </summary>
public static class KeyboardShortcut
{
    public static KeyGesture CreatePlatformCommandGesture(
        Key key,
        KeyModifiers additionalModifiers = KeyModifiers.None) =>
        new(key, GetPlatformCommandModifiers() | additionalModifiers);

    public static bool Matches(
        KeyEventArgs e,
        Key key,
        KeyModifiers modifiers = KeyModifiers.None) =>
        !e.Handled && e.Key == key && e.KeyModifiers == modifiers;

    public static bool Matches(KeyEventArgs e, IReadOnlyList<KeyGesture>? gestures)
    {
        if (e.Handled || gestures is null)
            return false;

        foreach (var gesture in gestures)
            if (gesture.Matches(e))
                return true;

        return false;
    }

    public static bool MatchesPlatformCommand(
        KeyEventArgs e,
        Key key,
        KeyModifiers additionalModifiers = KeyModifiers.None) =>
        Matches(e, key, GetPlatformCommandModifiers() | additionalModifiers);

    public static bool MatchesCopy(KeyEventArgs e) =>
        Application.Current?.PlatformSettings?.HotkeyConfiguration.Copy is { } gestures
            ? Matches(e, gestures)
            : MatchesPlatformCommand(e, Key.C);

    public static bool MatchesPaste(KeyEventArgs e) =>
        Application.Current?.PlatformSettings?.HotkeyConfiguration.Paste is { } gestures
            ? Matches(e, gestures)
            : MatchesPlatformCommand(e, Key.V);

    public static bool TryExecute(
        KeyEventArgs e,
        Key key,
        ICommand? command,
        object? commandParameter = null) =>
        Matches(e, key) && TryExecuteCommand(e, command, commandParameter);

    public static bool TryExecute(
        KeyEventArgs e,
        IReadOnlyList<KeyGesture>? gestures,
        ICommand? command,
        object? commandParameter = null) =>
        Matches(e, gestures) && TryExecuteCommand(e, command, commandParameter);

    public static bool TryExecutePlatformCommand(
        KeyEventArgs e,
        Key key,
        ICommand? command,
        object? commandParameter = null,
        KeyModifiers additionalModifiers = KeyModifiers.None) =>
        MatchesPlatformCommand(e, key, additionalModifiers)
        && TryExecuteCommand(e, command, commandParameter);

    public static bool TryExecuteCopy(
        KeyEventArgs e,
        ICommand? command,
        object? commandParameter = null) =>
        MatchesCopy(e) && TryExecuteCommand(e, command, commandParameter);

    public static bool TryExecutePaste(
        KeyEventArgs e,
        ICommand? command,
        object? commandParameter = null) =>
        MatchesPaste(e) && TryExecuteCommand(e, command, commandParameter);

    private static KeyModifiers GetPlatformCommandModifiers()
    {
        var modifiers = Application.Current?.PlatformSettings?.HotkeyConfiguration.CommandModifiers;
        return modifiers is null or KeyModifiers.None ? KeyModifiers.Control : modifiers.Value;
    }

    private static bool TryExecuteCommand(
        KeyEventArgs e,
        ICommand? command,
        object? commandParameter)
    {
        if (e.Handled || command?.CanExecute(commandParameter) is not true)
            return false;

        command.Execute(commandParameter);
        e.Handled = true;
        return true;
    }
}
