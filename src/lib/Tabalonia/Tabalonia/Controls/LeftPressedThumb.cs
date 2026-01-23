using Avalonia.Automation.Peers;


namespace Tabalonia.Controls;


public class LeftPressedThumb : TemplatedControl
{
    public static readonly RoutedEvent<VectorEventArgs> DragStartedEvent =
        RoutedEvent.Register<Thumb, VectorEventArgs>(nameof(DragStarted), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<VectorEventArgs> DragDeltaEvent =
        RoutedEvent.Register<Thumb, VectorEventArgs>(nameof(DragDelta), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<VectorEventArgs> DragCompletedEvent =
        RoutedEvent.Register<Thumb, VectorEventArgs>(nameof(DragCompleted), RoutingStrategies.Bubble);

    
    private Point? _lastPoint;
    private IPointer? _capturedPointer;
    

    /// <summary>
    /// The last known screen position of the pointer during a drag operation.
    /// Updated on each pointer move and release.
    /// </summary>
    internal PixelPoint LastScreenPosition { get; private set; }

    /// <summary>
    /// The pointer currently captured by this thumb, if any.
    /// Used by <see cref="TabsControl"/> to transfer capture during tear-off.
    /// </summary>
    internal IPointer? CapturedPointer => _capturedPointer;

    public event EventHandler<VectorEventArgs>? DragStarted
    {
        add => AddHandler(DragStartedEvent, value);
        remove => RemoveHandler(DragStartedEvent, value);
    }

    public event EventHandler<VectorEventArgs>? DragDelta
    {
        add => AddHandler(DragDeltaEvent, value);
        remove => RemoveHandler(DragDeltaEvent, value);
    }

    public event EventHandler<VectorEventArgs>? DragCompleted
    {
        add => AddHandler(DragCompletedEvent, value);
        remove => RemoveHandler(DragCompletedEvent, value);
    }


    protected override AutomationPeer OnCreateAutomationPeer() => new LeftPressedThumbPeer(this);

    
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (_lastPoint.HasValue)
        {
            var ev = new VectorEventArgs
            {
                RoutedEvent = DragCompletedEvent,
                Vector = _lastPoint.Value,
            };

            _lastPoint = null;
            _capturedPointer = null;

            RaiseEvent(ev);
        }
        
        base.OnPointerCaptureLost(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!IsLeftButtonPressed(e))
            return;
        
        if (_lastPoint.HasValue)
        {
            var currentPos = e.GetPosition(this);
            LastScreenPosition = this.PointToScreen(currentPos);

            var ev = new VectorEventArgs
            {
                RoutedEvent = DragDeltaEvent,
                Vector = currentPos - _lastPoint.Value,
            };

            RaiseEvent(ev);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (!IsLeftButtonPressed(e))
            return;
        
        e.Handled = true;
        _lastPoint = e.GetPosition(this);
        LastScreenPosition = this.PointToScreen(_lastPoint.Value);

        _capturedPointer = e.Pointer;
        e.Pointer.Capture(this);

        var ev = new VectorEventArgs
        {
            RoutedEvent = DragStartedEvent,
            Vector = (Vector)_lastPoint,
        };
        
        e.PreventGestureRecognition();

        RaiseEvent(ev);
    }
    
    
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (_lastPoint is null)
            return;

        e.Handled = true;
        var pos = e.GetPosition(this);
        LastScreenPosition = this.PointToScreen(pos);
        _lastPoint = null;
        _capturedPointer?.Capture(null);
        _capturedPointer = null;

        var ev = new VectorEventArgs
        {
            RoutedEvent = DragCompletedEvent,
            Vector = pos,
        };

        RaiseEvent(ev);
    }

    
    private bool IsLeftButtonPressed(PointerEventArgs args)
    {
        var point = args.GetCurrentPoint(this);
        
        return point.Properties.IsLeftButtonPressed;
    }
    
    
    private class LeftPressedThumbPeer : ControlAutomationPeer
    {
        public LeftPressedThumbPeer(LeftPressedThumb owner) : base(owner) { }
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Thumb;
        protected override bool IsContentElementCore() => false;
    }
}