using System.Diagnostics;
using Tabalonia.Events;


namespace Tabalonia.Controls;

[DebuggerDisplay("DragTabItem({Header})")]
public class DragTabItem : TabItem
{
    #region Private Fields

    private LeftPressedThumb _thumb = null!;
    
    private int _prevZindex;
    private int _logicalIndex;
    private bool _isDragging;
    private bool _isSiblingDragging;

    #endregion
    
    #region Avalonia Properties

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<DragTabItem, object?>(nameof(Icon));

    public static readonly StyledProperty<double> XProperty =
        AvaloniaProperty.Register<DragTabItem, double>(nameof(X));

    public static readonly StyledProperty<double> YProperty =
        AvaloniaProperty.Register<DragTabItem, double>(nameof(Y));

    public static readonly DirectProperty<DragTabItem, bool> IsDraggingProperty =
        AvaloniaProperty.RegisterDirect<DragTabItem, bool>(nameof(IsDragging),
            o => o.IsDragging, (o, v) => o.IsDragging = v);
    
    public static readonly DirectProperty<DragTabItem, int> LogicalIndexProperty =
        AvaloniaProperty.RegisterDirect<DragTabItem, int>(nameof(LogicalIndex),
            o => o.LogicalIndex, (o, v) => o.LogicalIndex = v);

    public static readonly DirectProperty<DragTabItem, bool> IsSiblingDraggingProperty =
        AvaloniaProperty.RegisterDirect<DragTabItem, bool>(nameof(IsSiblingDragging),
            o => o.IsSiblingDragging, (o, v) => o.IsSiblingDragging = v);

    #endregion

    #region Public Properties

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public double X
    {
        get => GetValue(XProperty);
        set => SetValue(XProperty, value);
    }

    public double Y
    {
        get => GetValue(YProperty);
        set => SetValue(YProperty, value);
    }

    public int LogicalIndex
    {
        get => _logicalIndex;
        internal set => SetAndRaise(LogicalIndexProperty, ref _logicalIndex, value);
    }
    
    public bool IsDragging
    {
        get => _isDragging;
        internal set => SetAndRaise(IsDraggingProperty, ref _isDragging, value);
    }

    public bool IsSiblingDragging
    {
        get => _isSiblingDragging;
        internal set => SetAndRaise(IsSiblingDraggingProperty, ref _isSiblingDragging, value);
    }

    #endregion

    #region Routed Events

    public static readonly RoutedEvent<RoutedEventArgs> MiddleClick =
        RoutedEvent.Register<DragTabItem, RoutedEventArgs>(nameof(MiddleClick), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<DragTabDragStartedEventArgs> DragStarted =
        RoutedEvent.Register<DragTabItem, DragTabDragStartedEventArgs>(nameof(DragStarted), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<DragTabDragDeltaEventArgs> DragDelta =
        RoutedEvent.Register<DragTabItem, DragTabDragDeltaEventArgs>(nameof(DragDelta), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<DragTabDragCompletedEventArgs> DragCompleted =
        RoutedEvent.Register<DragTabItem, DragTabDragCompletedEventArgs>(nameof(DragCompleted), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<DragTabDragDeltaEventArgs> PreviewDragDelta =
        RoutedEvent.Register<DragTabItem, DragTabDragDeltaEventArgs>(nameof(PreviewDragDelta), RoutingStrategies.Tunnel);
    
    #endregion

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var templateThumb = e.Find<LeftPressedThumb>("PART_Thumb");

        _thumb = templateThumb;
        _thumb.PointerReleased += ThumbOnPointerReleased;
        _thumb.DragStarted += ThumbOnDragStarted;
        _thumb.DragDelta += ThumbOnDragDelta;
        _thumb.DragCompleted += ThumbOnDragCompleted;
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        if (IsSelected || IsDragging)
            return;
        
        _prevZindex = ZIndex;
        
        ZIndex = ZIndexes.PointerOver;
    }
    
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        if (IsSelected || IsDragging)
            return;
        
        ZIndex = _prevZindex;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsSelectedProperty)
        {
            if (change.NewValue is true)
            {
                ZIndex = ZIndexes.Selected;
            }
        }
    }

    public override string ToString()
    {
        return $"{nameof(DragTabItem)}.{nameof(Header)}:{Header}";
    }

    private void ThumbOnPointerReleased(object? sender, PointerReleasedEventArgs args)
    {
        if (args.InitialPressMouseButton is MouseButton.Middle)
            RaiseEvent(new RoutedEventArgs(MiddleClick, this));
    }

    private void ThumbOnDragStarted(object? sender, VectorEventArgs args)
    {
        RaiseEvent(new DragTabDragStartedEventArgs(DragStarted, this, args));
    }
    
    private void ThumbOnDragDelta(object? sender, VectorEventArgs e)
    {
        var screenPosition = _thumb.LastScreenPosition;

        var previewEventArgs = new DragTabDragDeltaEventArgs(PreviewDragDelta, this, e)
        {
            PointerScreenPosition = screenPosition
        };
        RaiseEvent(previewEventArgs);
        // if (previewEventArgs.Cancel)
        //     _thumb.CancelDrag();
        if (!previewEventArgs.Handled)
        {
            var eventArgs = new DragTabDragDeltaEventArgs(DragDelta, this, e)
            {
                PointerScreenPosition = screenPosition
            };
            RaiseEvent(eventArgs);
            //if (eventArgs.Cancel)
            //    thumb.CancelDrag();
        }
    }

    private void ThumbOnDragCompleted(object? sender, VectorEventArgs e)
    {
        var args = new DragTabDragCompletedEventArgs(DragCompleted, this, e);
        RaiseEvent(args);
    }
}
