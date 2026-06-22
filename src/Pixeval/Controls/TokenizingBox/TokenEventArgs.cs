using System;

namespace Pixeval.Controls;

public class TokenEventArgs(object? item) : EventArgs
{
    public object? Item { get; } = item;
}