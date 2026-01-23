using Tabalonia.Controls;

namespace Tabalonia.Events;


public class DragTabDragDeltaEventArgs : DragTabItemEventArgs
{
    public DragTabDragDeltaEventArgs(DragTabItem dragTabItem, VectorEventArgs dragDeltaEventArgs)
        : base(dragTabItem)
    {
        DragDeltaEventArgs = dragDeltaEventArgs ?? throw new ArgumentNullException(nameof(dragDeltaEventArgs));
    }

    public DragTabDragDeltaEventArgs(RoutedEvent routedEvent, DragTabItem tabItem, VectorEventArgs dragDeltaEventArgs) 
        : base(routedEvent, tabItem)
    {
        DragDeltaEventArgs = dragDeltaEventArgs ?? throw new ArgumentNullException(nameof(dragDeltaEventArgs));
    }

    public DragTabDragDeltaEventArgs(RoutedEvent routedEvent, Interactive source, DragTabItem tabItem, VectorEventArgs dragDeltaEventArgs) 
        : base(routedEvent, source, tabItem)
    {
        DragDeltaEventArgs = dragDeltaEventArgs ?? throw new ArgumentNullException(nameof(dragDeltaEventArgs));
    }

    public VectorEventArgs DragDeltaEventArgs { get; }

    /// <summary>
    /// The pointer's position in screen coordinates at the time of this drag delta.
    /// </summary>
    public PixelPoint PointerScreenPosition { get; init; }

    public bool Cancel { get; set; }        
}