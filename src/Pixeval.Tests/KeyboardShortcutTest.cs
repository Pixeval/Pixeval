using System;
using System.Windows.Input;
using Avalonia.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Utilities;

namespace Pixeval.Tests;

[TestClass]
public sealed class KeyboardShortcutTest
{
    [TestMethod]
    public void MatchingGesture_ExecutesCommandAndHandlesEvent()
    {
        var command = new RecordingCommand();
        var args = CreateArgs(Key.Right);

        var executed = KeyboardShortcut.TryExecute(args, Key.Right, command, "parameter");

        Assert.IsTrue(executed);
        Assert.IsTrue(args.Handled);
        Assert.AreEqual(1, command.ExecutionCount);
        Assert.AreEqual("parameter", command.Parameter);
    }

    [TestMethod]
    public void HandledEvent_DoesNotExecuteParentShortcut()
    {
        var command = new RecordingCommand();
        var args = CreateArgs(Key.V, KeyModifiers.Control);
        args.Handled = true;

        var executed = KeyboardShortcut.TryExecutePaste(args, command);

        Assert.IsFalse(executed);
        Assert.AreEqual(0, command.ExecutionCount);
    }

    [TestMethod]
    public void DisabledCommand_DoesNotHandleEvent()
    {
        var command = new RecordingCommand(false);
        var args = CreateArgs(Key.Left);

        var executed = KeyboardShortcut.TryExecute(args, Key.Left, command);

        Assert.IsFalse(executed);
        Assert.IsFalse(args.Handled);
        Assert.AreEqual(0, command.ExecutionCount);
    }

    [TestMethod]
    public void PlatformCommandGesture_RequiresExactModifiers()
    {
        var gesture = KeyboardShortcut.CreatePlatformCommandGesture(Key.S, KeyModifiers.Shift);
        var matching = CreateArgs(Key.S, gesture.KeyModifiers);
        var missingCommandModifier = CreateArgs(Key.S, KeyModifiers.Shift);
        var extraModifier = CreateArgs(Key.S, gesture.KeyModifiers | KeyModifiers.Alt);

        Assert.AreEqual(KeyModifiers.Control | KeyModifiers.Shift, gesture.KeyModifiers);
        Assert.IsTrue(gesture.Matches(matching));
        Assert.IsFalse(gesture.Matches(missingCommandModifier));
        Assert.IsFalse(gesture.Matches(extraModifier));
    }

    private static KeyEventArgs CreateArgs(
        Key key,
        KeyModifiers modifiers = KeyModifiers.None) =>
        new()
        {
            Key = key,
            KeyModifiers = modifiers
        };

    private sealed class RecordingCommand(bool canExecute = true) : ICommand
    {
        public int ExecutionCount { get; private set; }

        public object? Parameter { get; private set; }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => canExecute;

        public void Execute(object? parameter)
        {
            ++ExecutionCount;
            Parameter = parameter;
        }
    }
}
