using Tabalonia.Controls;

namespace Tabalonia.Events;


public class DragTabDragStartedEventArgs : DragTabItemEventArgs
{
    public DragTabDragStartedEventArgs(DragTabItem dragTabItem, VectorEventArgs dragStartedEventArgs)
        : base(dragTabItem)
    {
        //DragStartedEventArgs = dragStartedEventArgs ?? throw new ArgumentNullException(nameof(dragStartedEventArgs));
    }

    public DragTabDragStartedEventArgs(RoutedEvent routedEvent, DragTabItem tabItem, VectorEventArgs dragStartedEventArgs)
        : base(routedEvent, tabItem)
    {
        //DragStartedEventArgs = dragStartedEventArgs;
    }

    public DragTabDragStartedEventArgs(RoutedEvent routedEvent, Interactive source, DragTabItem tabItem, VectorEventArgs dragStartedEventArgs)
        : base(routedEvent, source, tabItem)
    {
        //DragStartedEventArgs = dragStartedEventArgs;
    }

    //public VectorEventArgs DragStartedEventArgs { get; }
}