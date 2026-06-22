using System.ComponentModel;

namespace Pixeval.Controls;

public class TokenAddingEventArgs(object? item, string? tokenText = null) : CancelEventArgs
{
    public string? TokenText { get; } = tokenText;

    public object? Item { get; set; } = item;
}