using Tabalonia.Controls;

namespace Tabalonia.Events;


public class DragTabDragCompletedEventArgs : DragTabItemEventArgs
{
    public DragTabDragCompletedEventArgs(DragTabItem dragItem, VectorEventArgs dragCompletedEventArgs) 
        :base(dragItem)
    {
        //DragCompletedEventArgs = dragCompletedEventArgs ?? throw new ArgumentNullException(nameof(dragCompletedEventArgs));
    }

    public DragTabDragCompletedEventArgs(RoutedEvent routedEvent, DragTabItem dragItem, VectorEventArgs dragCompletedEventArgs)
        : base(routedEvent, dragItem)
    {
        //DragCompletedEventArgs = dragCompletedEventArgs ?? throw new ArgumentNullException(nameof(dragCompletedEventArgs));
    }

    public DragTabDragCompletedEventArgs(RoutedEvent routedEvent, Interactive source, DragTabItem dragItem, VectorEventArgs dragCompletedEventArgs)
        : base(routedEvent, source, dragItem)
    {
        
        //DragCompletedEventArgs = dragCompletedEventArgs ?? throw new ArgumentNullException(nameof(dragCompletedEventArgs));
    }


    //public VectorEventArgs DragCompletedEventArgs { get; }
}