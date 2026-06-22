using System.ComponentModel;

namespace Pixeval.Controls;

public class TokenRemovingEventArgs(object? item) : CancelEventArgs
{
    public object? Item { get; } = item;
}