using System.ComponentModel;
using Tabalonia.Controls;

namespace Tabalonia.Events;

public class TabClosingEventArgs(object? item, DragTabItem tab) : CancelEventArgs
{
    public object? Item { get; } = item;

    public DragTabItem Tab { get; } = tab;
}
