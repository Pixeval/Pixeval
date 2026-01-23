using Tabalonia.Controls;

namespace Tabalonia.Events;

public class TabClosedEventArgs(object? item, DragTabItem tab) : EventArgs
{
    public object? Item { get; } = item;

    public DragTabItem Tab { get; } = tab;
}
