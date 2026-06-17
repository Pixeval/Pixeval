using System.Collections;
using System.Windows.Input;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Shapes;
using Avalonia.VisualTree;
using Tabalonia.Events;
using Tabalonia.InterTab;
using Tabalonia.Panels;

namespace Tabalonia.Controls;

public class TabsControl : TabControl
{
    #region Constants

    private const double DefaultTabWidth = 140;

    public const double WindowsAndLinuxDefaultLeftThumbWidth = 4d;
    public const double MacOsDefaultLeftThumbWidth = 80d;

    public const double WindowsDefaultRightThumbWidth = 160d;
    public const double MacOsDefaultRightThumbWidth = 50d;

    #endregion

    #region Private Fields

    private readonly TabsPanel _tabsPanel;
    private DragTabItem? _draggedItem;
    private bool _dragging;
    private Control? _leftHeaderContentDraggable;
    private Control? _rightHeaderContentDraggable;

    // Inter-tab state: active during a tear-off / floating window drag
    private FloatingDragState? _floatingState;

    // Pointer position relative to DragTabItem at drag start (DIPs).
    // Captured once so that tear-off preserves the original grab point.
    private Point _dragStartPointerInTab;

    #endregion

    #region Static Instance Tracking

    private static readonly HashSet<TabsControl> s_loadedInstances = new();

    /// <summary>
    /// All currently loaded (attached to visual tree) <see cref="TabsControl"/> instances.
    /// Used for finding merge targets during inter-tab drag.
    /// </summary>
    internal static IReadOnlyCollection<TabsControl> LoadedInstances => s_loadedInstances;

    #endregion

    #region Avalonia Properties

    public static readonly StyledProperty<double> AdjacentHeaderItemOffsetProperty =
        AvaloniaProperty.Register<TabsControl, double>(nameof(AdjacentHeaderItemOffset), defaultValue: 0);

    public static readonly StyledProperty<double> TabItemWidthProperty =
        AvaloniaProperty.Register<TabsControl, double>(nameof(TabItemWidth), defaultValue: DefaultTabWidth);

    public static readonly StyledProperty<bool> ShowDefaultCloseButtonProperty =
        AvaloniaProperty.Register<TabsControl, bool>(nameof(ShowDefaultCloseButton), defaultValue: true);

    public static readonly StyledProperty<bool> ShowDefaultAddButtonProperty =
        AvaloniaProperty.Register<TabsControl, bool>(nameof(ShowDefaultAddButton), defaultValue: true);

    public static readonly StyledProperty<int> FixedHeaderCountProperty =
        AvaloniaProperty.Register<TabsControl, int>(nameof(FixedHeaderCount), defaultValue: 0);

    public static readonly StyledProperty<double> LeftIndentProperty =
        AvaloniaProperty.Register<TabsControl, double>(nameof(LeftIndent),
            defaultValue: OperatingSystem.IsMacOS() ? MacOsDefaultLeftThumbWidth : WindowsAndLinuxDefaultLeftThumbWidth);

    public static readonly StyledProperty<double> RightIndentProperty =
        AvaloniaProperty.Register<TabsControl, double>(nameof(RightIndent),
            defaultValue: OperatingSystem.IsWindows() ? WindowsDefaultRightThumbWidth : MacOsDefaultRightThumbWidth);

    public static readonly StyledProperty<object?> LeftContentProperty =
        AvaloniaProperty.Register<TabsControl, object?>(nameof(LeftContent));

    public static readonly StyledProperty<object?> LeftHeaderContentProperty =
        AvaloniaProperty.Register<TabsControl, object?>(nameof(LeftHeaderContent));
    
    public static readonly StyledProperty<object?> RightHeaderContentProperty =
        AvaloniaProperty.Register<TabsControl, object?>(nameof(RightHeaderContent));

    public static readonly StyledProperty<bool> LeftHeaderContentDraggableProperty =
        AvaloniaProperty.Register<TabsControl, bool>(nameof(LeftHeaderContentDraggable));
    
    public static readonly StyledProperty<bool> RightHeaderContentDraggableProperty =
        AvaloniaProperty.Register<TabsControl, bool>(nameof(RightHeaderContentDraggable));

    public static readonly StyledProperty<InterTabController?> InterTabControllerProperty =
        AvaloniaProperty.Register<TabsControl, InterTabController?>(nameof(InterTabController));
    
    #endregion

    #region Constructor

    public TabsControl()
    {
        AddHandler(DragTabItem.MiddleClick, ItemMiddleClick, handledEventsToo: true);
        AddHandler(DragTabItem.DragStarted, ItemDragStarted, handledEventsToo: true);
        AddHandler(DragTabItem.DragDelta, ItemDragDelta);
        AddHandler(DragTabItem.DragCompleted, ItemDragCompleted, handledEventsToo: true);

        _tabsPanel = new TabsPanel(this)
        {
            ItemWidth = TabItemWidth,
            ItemOffset = AdjacentHeaderItemOffset
        };

        _tabsPanel.DragCompleted += TabsPanelOnDragCompleted;

        ItemsPanel = new FuncTemplate<Panel?>(() => _tabsPanel);

        AddItemCommand = new SimpleActionCommand(AddItem);
        CloseItemCommand = new SimpleParamActionCommand(CloseItem);
    }

    private void ItemMiddleClick(object? sender, RoutedEventArgs e)
    {
        CloseItem(e.Source);
    }

    #endregion

    #region Public Properties
    
    public double AdjacentHeaderItemOffset
    {
        get => GetValue(AdjacentHeaderItemOffsetProperty);
        set => SetValue(AdjacentHeaderItemOffsetProperty, value);
    }

    public double TabItemWidth
    {
        get => GetValue(TabItemWidthProperty);
        set => SetValue(TabItemWidthProperty, value);
    }

    public bool ShowDefaultCloseButton
    {
        get => GetValue(ShowDefaultCloseButtonProperty);
        set => SetValue(ShowDefaultCloseButtonProperty, value);
    }

    public bool ShowDefaultAddButton
    {
        get => GetValue(ShowDefaultAddButtonProperty);
        set => SetValue(ShowDefaultAddButtonProperty, value);
    }

    public event EventHandler<TabsControl, EventArgs>? AddTabButtonClick;

    public event EventHandler<TabsControl, TabClosingEventArgs>? TabClosing;

    public event EventHandler<TabsControl, TabClosedEventArgs>? TabClosed;

    /// <summary>
    /// Allows a first adjacent tabs to be fixed (no dragging, and default close button will not show).
    /// </summary>
    public int FixedHeaderCount
    {
        get => GetValue(FixedHeaderCountProperty);
        set => SetValue(FixedHeaderCountProperty, value);
    }

    public double LeftIndent
    {
        get => GetValue(LeftIndentProperty);
        set => SetValue(LeftIndentProperty, value);
    }

    public double RightIndent
    {
        get => GetValue(RightIndentProperty);
        set => SetValue(RightIndentProperty, value);
    }

    internal ICommand AddItemCommand { get; }

    internal ICommand CloseItemCommand { get; }

    public object? LeftContent
    {
        get => GetValue(LeftContentProperty);
        set => SetValue(LeftContentProperty, value);
    }

    public object? LeftHeaderContent
    {
        get => GetValue(LeftHeaderContentProperty);
        set => SetValue(LeftHeaderContentProperty, value);
    }
    
    public object? RightHeaderContent
    {
        get => GetValue(RightHeaderContentProperty);
        set => SetValue(RightHeaderContentProperty, value);
    }

    public bool LeftHeaderContentDraggable
    {
        get => GetValue(LeftHeaderContentDraggableProperty);
        set => SetValue(LeftHeaderContentDraggableProperty, value);
    }
    
    public bool RightHeaderContentDraggable
    {
        get => GetValue(RightHeaderContentDraggableProperty);
        set => SetValue(RightHeaderContentDraggableProperty, value);
    }

    /// <summary>
    /// Configuration for inter-tab (cross-window) dragging. Set to a non-null value to enable
    /// tearing tabs out to new windows and merging tabs between windows.
    /// </summary>
    public InterTabController? InterTabController
    {
        get => GetValue(InterTabControllerProperty);
        set => SetValue(InterTabControllerProperty, value);
    }
    
    #endregion

    #region Protected Methods

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        AddDragHandler(e.NameScope.Get<Rectangle>("PART_LeftDragWindowThumb"));
        AddDragHandler(e.NameScope.Get<Rectangle>("PART_RightDragWindowThumb"));
        AddDragHandler(e.NameScope.Get<Rectangle>("PART_MiddleDragWindowThumb"));
        _leftHeaderContentDraggable = e.NameScope.Find<ContentPresenter>("PART_LeftHeaderContent");
        if (LeftHeaderContentDraggable)
            AddDragHandler(_leftHeaderContentDraggable);
        _rightHeaderContentDraggable = e.NameScope.Find<ContentPresenter>("PART_RightHeaderContent");
        if (RightHeaderContentDraggable)
            AddDragHandler(_rightHeaderContentDraggable);
    }

    private void AddDragHandler(Control? control)
    {
        if (control is null)
            return;

        control.AddHandler(PointerPressedEvent, OnThumbBeginDrag, handledEventsToo: true);
        control.DoubleTapped += WindowDragThumbOnDoubleTapped;
    }

    private void RemoveDragHandler(Control? control)
    {
        if (control is null)
            return;

        control.RemoveHandler(PointerPressedEvent, OnThumbBeginDrag);
        control.DoubleTapped -= WindowDragThumbOnDoubleTapped;
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) =>
        new DragTabItem();

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == AdjacentHeaderItemOffsetProperty)
        {
            _tabsPanel.ItemOffset = AdjacentHeaderItemOffset;
        }
        else if (change.Property == TabItemWidthProperty)
        {
            _tabsPanel.ItemWidth = TabItemWidth;
        }
        else if (change.Property == LeftHeaderContentDraggableProperty)
        {
            if (LeftHeaderContentDraggable)
                AddDragHandler(_leftHeaderContentDraggable);
            else
                RemoveDragHandler(_leftHeaderContentDraggable);
        }
        else if (change.Property == RightHeaderContentDraggableProperty)
        {
            if (LeftHeaderContentDraggable)
                AddDragHandler(_rightHeaderContentDraggable);
            else
                RemoveDragHandler(_rightHeaderContentDraggable);
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        s_loadedInstances.Add(this);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        s_loadedInstances.Remove(this);
        base.OnDetachedFromVisualTree(e);
    }

    #endregion

    private void SetSelectedNewTab(IList items, int removedItemIndex) =>
        SelectedItem = removedItemIndex == items.Count ? items[^1] : items[removedItemIndex];

    private IEnumerable<DragTabItem> DragTabItems()
    {
        foreach (var item in Items)
        {
            var container = ContainerFromItem(item!);

            if (container is DragTabItem dragTabItem)
                yield return dragTabItem;
        }
    }

    private void ItemDragStarted(object? sender, DragTabDragStartedEventArgs e)
    {
        _draggedItem = e.TabItem;

        e.Handled = true;

        _draggedItem.IsSelected = true;

        // Record pointer position relative to the tab at drag start.
        // This is used later by MonitorBreach so that the new window places
        // the tab under the cursor at the same offset as the original grab.
        var thumb = _draggedItem.FindDescendantOfType<LeftPressedThumb>();
        if (thumb is not null)
        {
            try
            {
                var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0;
                var cursorScreen = thumb.LastScreenPosition;
                var tabScreen = _draggedItem.PointToScreen(new Point(0, 0));
                _dragStartPointerInTab = new Point(
                    (cursorScreen.X - tabScreen.X) / scaling,
                    (cursorScreen.Y - tabScreen.Y) / scaling);
            }
            catch
            {
                _dragStartPointerInTab = new Point(_draggedItem.Bounds.Width / 2, _draggedItem.Bounds.Height / 2);
            }
        }

        if (ItemFromContainer(_draggedItem) is { } item)
        {
            if (item is TabItem tabItem)
                tabItem.IsSelected = true;

            SelectedItem = item;
        }
    }

    private void ItemDragDelta(object? sender, DragTabDragDeltaEventArgs e)
    {
        if (_draggedItem is null || _floatingState is not null)
        {
            // After tear-off, _draggedItem is null and pointer tracking
            // is handled by OnPointerMoved instead of DragDelta events.
            e.Handled = true;
            return;
        }

        if (_draggedItem.LogicalIndex < FixedHeaderCount)
        {
            e.Handled = true;
            return;
        }

        // Single-tab windows: immediately start window drag for merge capability
        if (Items.Count < 2 && InterTabController is not null)
        {
            StartSingleTabWindowDrag(e);
            e.Handled = true;
            return;
        }

        if (!_dragging)
        {
            _dragging = true;
            SetDraggingItem(_draggedItem);
        }

        _draggedItem.X += e.DragDeltaEventArgs.Vector.X;
        _draggedItem.Y += e.DragDeltaEventArgs.Vector.Y;

        if (InterTabController is { } controller)
        {
            // Check if pointer has breached the tab header boundary → tear off
            MonitorBreach(e.PointerScreenPosition, controller, false);
        }

        Dispatcher.UIThread.Post(() => _tabsPanel.InvalidateMeasure(), DispatcherPriority.Loaded);

        e.Handled = true;
    }

    private void ItemDragCompleted(object? sender, DragTabDragCompletedEventArgs e)
    {
        if (_floatingState is not null)
        {
            // DragCompleted fired because we transferred capture from LeftPressedThumb
            // to TabsControl in MonitorBreach. Don't do normal cleanup — MonitorBreach
            // is still running and OnPointerMoved/Released will handle the rest.
            e.Handled = true;
            return;
        }

        foreach (var item in DragTabItems())
        {
            item.IsDragging = false;
            item.IsSiblingDragging = false;
        }

        Dispatcher.UIThread.Post(() => _tabsPanel.InvalidateMeasure(), DispatcherPriority.Loaded);

        _dragging = false;
    }

    private void SetDraggingItem(DragTabItem draggedItem)
    {
        foreach (var item in DragTabItems())
        {
            item.IsDragging = false;
            item.IsSiblingDragging = true;
        }

        draggedItem.IsDragging = true;
        draggedItem.IsSiblingDragging = false;
    }

    private void TabsPanelOnDragCompleted()
    {
        MoveTabModelsIfNeeded();

        _draggedItem = null;
    }

    private void MoveTabModelsIfNeeded()
    {
        var draggedItem = _draggedItem!;

        if (ItemFromContainer(draggedItem) is not { } item)
            return;
        if (ItemsSource is not IList list)
            return;
        if (draggedItem.LogicalIndex == list.IndexOf(item))
            return;
        list.Remove(item);
        list.Insert(draggedItem.LogicalIndex, item);

        SelectedItem = item;

        int i = 0;

        foreach (var dragTabItem in DragTabItems())
            dragTabItem.LogicalIndex = i++;
    }

    private void OnThumbBeginDrag(object? sender, PointerPressedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is not Window window)
            return;
        var currentPoint = e.GetCurrentPoint(this);
        if (currentPoint.Properties.IsLeftButtonPressed ||
            currentPoint.Pointer.Type is PointerType.Touch)
        {
            window.BeginMoveDrag(e);
        }
    }

    private void WindowDragThumbOnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        var window = this.FindLogicalAncestorOfType<Window>();

        window?.RestoreWindow();
    }

    private void AddItem() => AddTabButtonClick?.Invoke(this, EventArgs.Empty);

    private void CloseItem(object? tabItemSource)
    {
        ArgumentNullException.ThrowIfNull(tabItemSource);

        if (tabItemSource is not DragTabItem tabItem)
            return;

        RemoveItem(tabItem);
    }

    private void RemoveItem(DragTabItem container)
    {
        var item = ItemFromContainer(container);
        var removedItemIndex = Items.IndexOf(container);
        if (removedItemIndex is -1)
            return;

        TabClosingEventArgs tabClosingEventArgs = new(item, container);
        TabClosing?.Invoke(this, tabClosingEventArgs);
        if (tabClosingEventArgs.Cancel)
            return;

        var removedItemIsSelected = SelectedItem == item;
        Items.Remove(item);
        var closedEventArgs = new TabClosedEventArgs(item, container);
        TabClosed?.Invoke(this, closedEventArgs);
        if (Items.Count is 0)
        {
            this.FindLogicalAncestorOfType<Window>()?.Close();
        }
        else if (removedItemIsSelected)
            SetSelectedNewTab(Items, removedItemIndex);
    }

    #region Inter-Tab Drag Support

    /// <summary>
    /// Checks whether the pointer's screen position (<paramref name="screenPoint"/>)
    /// is inside this <see cref="TabsControl"/>'s tab header row area.
    /// Uses the TopPanel's full bounds so that empty space in the tab strip is also a valid drop zone.
    /// </summary>
    internal bool IsPointInTabHeaderArea(PixelPoint screenPoint)
    {
        if (!IsVisible || !IsArrangeValid)
            return false;

        // Prefer TopPanel (the full header row) over _tabsPanel (just the tab items)
        var headerPanel = _tabsPanel.FindAncestorOfType<TopPanel>() as Control ?? _tabsPanel;

        if (!headerPanel.IsArrangeValid)
            return false;

        try
        {
            var topLeft = headerPanel.PointToScreen(new Point(0, 0));
            var bottomRight = headerPanel.PointToScreen(
                new Point(headerPanel.Bounds.Width, headerPanel.Bounds.Height));

            return screenPoint.X >= topLeft.X && screenPoint.X <= bottomRight.X
                && screenPoint.Y >= topLeft.Y && screenPoint.Y <= bottomRight.Y;
        }
        catch
        {
            // PointToScreen can throw if not attached to a visual root
            return false;
        }
    }

    /// <summary>
    /// When only one tab remains, dragging it moves the entire window.
    /// This allows the user to merge a single-tab window into another window
    /// by dragging it over the target's tab header area.
    /// </summary>
    private void StartSingleTabWindowDrag(DragTabDragDeltaEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not Window sourceWindow)
            return;

        var draggedItem = _draggedItem!;

        // Get the pointer from LeftPressedThumb so we can transfer capture
        var thumb = draggedItem.FindDescendantOfType<LeftPressedThumb>();
        var pointer = thumb?.CapturedPointer;
        if (pointer is null)
            return;

        var cursorScreen = e.PointerScreenPosition;
        var dragOffset = new Point(
            cursorScreen.X - sourceWindow.Position.X,
            cursorScreen.Y - sourceWindow.Position.Y);

        // Set floating state BEFORE capture transfer (same pattern as MonitorBreach).
        // pointer.Capture(this) will synchronously fire DragCompleted on LeftPressedThumb;
        // ItemDragCompleted checks _floatingState and returns early.
        _floatingState = new FloatingDragState
        {
            FloatingWindow = sourceWindow,
            FloatingTabsControl = this,
            DragOffset = dragOffset,
            IsSelfDrag = true
        };

        // Transfer pointer capture: LeftPressedThumb → TabsControl
        pointer.Capture(this);

        // Reset tab drag state — the tab is no longer "being dragged" visually
        _draggedItem = null;
        _dragging = false;
        foreach (var di in DragTabItems())
        {
            di.IsDragging = false;
            di.IsSiblingDragging = false;
            di.X = 0;
            di.Y = 0;
        }

        Dispatcher.UIThread.Post(() => _tabsPanel.InvalidateMeasure(), DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Detects if the pointer has moved outside the tab header area by the configured grace distance.
    /// If so, initiates a tear-off: transfers pointer capture to this <see cref="TabsControl"/>,
    /// removes the tab from source, creates a floating window, and adds the tab to it.
    /// From that point on, <see cref="OnPointerMoved"/> and <see cref="OnPointerReleased"/>
    /// handle floating window tracking and merge completion.
    /// </summary>
    /// 
    /// <remarks>
    /// Checks if the pointer has moved outside the tab header area during a continued
    /// merged-item drag. If so, tears the tab off into a new floating window.
    /// Similar to <see cref="MonitorBreach"/> but works when the pointer is already
    /// captured to this <see cref="TabsControl"/> (not to a <see cref="LeftPressedThumb"/>).
    /// </remarks>
    private void MonitorBreach(PixelPoint cursorScreen, InterTabController controller, bool fromMergedDrag)
    {
        if (Items.Count < 2)
            return; // Don't tear off the last tab; single-tab windows use native window drag.

        // Calculate pointer position relative to the tab header panel (in DIPs)
        PixelPoint panelOriginScreen;
        try
        {
            panelOriginScreen = _tabsPanel.PointToScreen(new Point(0, 0));
        }
        catch
        {
            return;
        }

        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0;
        var cursorInPanel = new Point(
            (cursorScreen.X - panelOriginScreen.X) / scaling,
            (cursorScreen.Y - panelOriginScreen.Y) / scaling);

        var hGrace = controller.HorizontalPopoutGrace;
        var vGrace = controller.VerticalPopoutGrace;

        bool hasBreach =
            cursorInPanel.X < -hGrace ||
            cursorInPanel.X > _tabsPanel.Bounds.Width + hGrace ||
            cursorInPanel.Y < -vGrace ||
            cursorInPanel.Y > _tabsPanel.Bounds.Height + vGrace;

        if (!hasBreach)
            return;

        // === Breach detected: initiate tear-off (Edge-style) ===
        if (TopLevel.GetTopLevel(this) is not Window sourceWindow)
            return;

        var draggedItem = _draggedItem!;

        // Get the data item to transfer
        if (ItemFromContainer(draggedItem) is not { } item)
            return;

        IPointer? pointer = null;
        if (!fromMergedDrag)
        {
            // Get the pointer from LeftPressedThumb so we can transfer capture
            var thumb = draggedItem.FindDescendantOfType<LeftPressedThumb>();
            pointer = thumb?.CapturedPointer;
            if (pointer is null)
                return;
        }

        // Use the pointer-in-tab offset captured at drag start, so the
        // new window positions the tab under the cursor at the original grab point.
        var pointerInTab = _dragStartPointerInTab;

        // In the new window the tab will be offset from the window's top-left by
        // the same header structure (LeftThumb + LeftHeaderContent).  Estimate that offset
        // from the source layout so we can place the window correctly.
        Point tabOriginInWindow;
        try
        {
            var panelScreenOrigin = _tabsPanel.PointToScreen(new Point(0, 0));
            tabOriginInWindow = new Point(
                (panelScreenOrigin.X - sourceWindow.Position.X) / scaling,
                (panelScreenOrigin.Y - sourceWindow.Position.Y) / scaling);
        }
        catch
        {
            tabOriginInWindow = new Point(LeftIndent, 0);
        }

        // dragOffset (in pixels) = distance from new window top-left to cursor
        var dragOffset = new Point(
            (tabOriginInWindow.X + pointerInTab.X) * scaling,
            (tabOriginInWindow.Y + pointerInTab.Y) * scaling);

        double newWidth, newHeight;
        if (sourceWindow.WindowState is WindowState.Maximized)
        {
            newWidth = double.Min(800, sourceWindow.Bounds.Width);
            newHeight = double.Min(600, sourceWindow.Bounds.Height);
        }
        else
        {
            newWidth = sourceWindow.Width;
            newHeight = sourceWindow.Height;
        }

        // Create the new host window
        TabHost newTabHost;
        try
        {
            newTabHost = controller.InterTabClient.GetNewHost(controller, new TabHost(sourceWindow, this));
        }
        catch
        {
            return; // client couldn't create a host; silently abort
        }

        var floatingWindow = newTabHost.Window;
        var floatingTabsControl = newTabHost.TabsControl;

        // Propagate inter-tab configuration so the new window can also tear off & merge
        floatingTabsControl.InterTabController ??= new InterTabController
        {
            InterTabClient = controller.InterTabClient,
            Partition = controller.Partition,
            HorizontalPopoutGrace = controller.HorizontalPopoutGrace,
            VerticalPopoutGrace = controller.VerticalPopoutGrace
        };

        floatingWindow.Width = newWidth;
        floatingWindow.Height = newHeight;
        floatingWindow.WindowStartupLocation = WindowStartupLocation.Manual;
        floatingWindow.Position = new PixelPoint(
            (int)(cursorScreen.X - dragOffset.X),
            (int)(cursorScreen.Y - dragOffset.Y));

        // *** Set floating state BEFORE capture transfer ***
        // pointer.Capture(this) will synchronously fire DragCompleted on LeftPressedThumb,
        // which bubbles to ItemDragCompleted. That handler checks _floatingState and returns early.
        _floatingState = new FloatingDragState
        {
            FloatingWindow = floatingWindow,
            FloatingTabsControl = floatingTabsControl,
            DragOffset = dragOffset
        };

        if (!fromMergedDrag)
            // Transfer pointer capture from LeftPressedThumb → TabsControl.
            // This fires DragCompleted on the thumb synchronously (handled above).
            // After this, OnPointerMoved / OnPointerReleased on TabsControl drive the floating drag.
            pointer!.Capture(this);

        // Remove the item from the source TabsControl
        var removedIndex = Items.IndexOf(item);
        Items.Remove(item);

        // Reset drag state on the transferred item so the target's TabsPanel
        // uses normal (non-drag) measurement/arrangement paths
        if (item is DragTabItem transferredTab)
        {
            transferredTab.IsDragging = false;
            transferredTab.IsSiblingDragging = false;
            transferredTab.X = 0;
            transferredTab.Y = 0;
        }

        // Reset drag state on the source: the item is gone
        _draggedItem = null;
        _dragging = false;
        foreach (var di in DragTabItems())
        {
            di.IsDragging = false;
            di.IsSiblingDragging = false;
        }

        // Handle source remaining tabs selection (or close if empty)
        HandleSourceEmptied(removedIndex);

        // Show the floating window and add the item after layout is ready
        floatingWindow.Show();

        // Force source layout recalculation for remaining tabs
        InvalidateTabLayout();

        Dispatcher.UIThread.Post(() =>
        {
            floatingTabsControl.Items.Add(item);
            floatingTabsControl.SelectedItem = item;
            floatingTabsControl.InvalidateTabLayout();
        }, DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Tracks the floating window position while the pointer is captured to this TabsControl.
    /// When the floating window enters another TabsControl's tab header area, immediately
    /// merges the tab into the target and transfers drag control there.
    /// </summary>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_floatingState is { } fs)
        {
            HandleFloatingPointerMoved(e, fs);
            return;
        }

        // Continue-drag after merge: _draggedItem is set and pointer is captured to this TC
        if (_draggedItem is not null && _dragging)
        {
            HandleMergedDragPointerMoved(e);
        }
    }

    private void HandleFloatingPointerMoved(PointerEventArgs e, FloatingDragState fs)
    {
        PixelPoint cursorScreen;
        try
        {
            cursorScreen = this.PointToScreen(e.GetPosition(this));
        }
        catch
        {
            return;
        }

        fs.FloatingWindow.Position = new PixelPoint(
            (int)(cursorScreen.X - fs.DragOffset.X),
            (int)(cursorScreen.Y - fs.DragOffset.Y));

        // Check if we're over a merge target
        var mergeTarget = FindReentryTarget(cursorScreen, InterTabController?.Partition, fs.FloatingTabsControl);
        if (mergeTarget is null)
            return;

        // === Immediate merge: transfer the tab and continue dragging in the target ===
        var sourceTC = fs.FloatingTabsControl;
        if (sourceTC.Items.Count == 0)
            return;

        var item = sourceTC.Items[0]!;
        sourceTC.Items.Remove(item);

        // Reset drag visual state on the transferred item
        if (item is DragTabItem tabToMerge)
        {
            tabToMerge.IsDragging = false;
            tabToMerge.IsSiblingDragging = false;
            tabToMerge.X = 0;
            tabToMerge.Y = 0;
        }

        // Clear floating state on source (us) BEFORE releasing capture
        var windowToClose = fs.FloatingWindow;
        _floatingState = null;

        // Release capture from source TabsControl
        e.Pointer.Capture(null);

        // All operations on mergeTarget must be deferred because it belongs
        // to a different window with its own LayoutManager. Performing layout
        // invalidation synchronously here would cause
        // "InvalidateArrange on wrong LayoutManager".
        var pointer = e.Pointer;
        Dispatcher.UIThread.Post(() =>
        {
            // Activate the target window (bring to front)
            if (TopLevel.GetTopLevel(mergeTarget) is Window targetWindow)
                targetWindow.Activate();

            // Close the now-empty floating window
            windowToClose.Close();

            // Start dragging the merged tab in the target TabsControl.
            // ReceiveMergedItemDrag handles Items.Insert, layout, and pointer capture.
            mergeTarget.ReceiveMergedItemDrag(item, pointer, cursorScreen, _dragStartPointerInTab);
        }, DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Handles pointer movement during a continued drag after merge.
    /// Moves the dragged tab horizontally and checks for re-tear-off.
    /// </summary>
    private void HandleMergedDragPointerMoved(PointerEventArgs e)
    {
        var draggedItem = _draggedItem!;
        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0;

        PixelPoint cursorScreen;
        try
        {
            cursorScreen = this.PointToScreen(e.GetPosition(this));
        }
        catch
        {
            return;
        }

        // Compute new X for the dragged tab (relative to TabsPanel)
        try
        {
            var panelScreen = _tabsPanel.PointToScreen(new Point(0, 0));
            draggedItem.X = ((cursorScreen.X - panelScreen.X) / scaling) - _dragStartPointerInTab.X;
        }
        catch
        {
            // Can't compute; leave as-is
        }

        // Check if pointer has breached the header boundary → re-tear-off
        if (InterTabController is { } controller)
        {
            MonitorBreach(cursorScreen, controller, true);
        }

        Dispatcher.UIThread.Post(() => _tabsPanel.InvalidateMeasure(), DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Completes the floating drag when the pointer is released.
    /// The floating window stays as a new independent window (merge happens
    /// immediately in <see cref="OnPointerMoved"/> when entering a target).
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_floatingState is not null)
        {
            _floatingState = null;
            e.Handled = true;
            e.Pointer.Capture(null);
            return;
        }

        // Complete a continued-drag after merge
        if (_draggedItem is not null && _dragging)
        {
            e.Pointer.Capture(null);
            e.Handled = true;

            foreach (var item in DragTabItems())
            {
                item.IsDragging = false;
                item.IsSiblingDragging = false;
            }

            _dragging = false;

            Dispatcher.UIThread.Post(() => _tabsPanel.InvalidateMeasure(), DispatcherPriority.Loaded);
        }
    }

    /// <summary>
    /// Handles unexpected capture loss during a floating drag.
    /// Leaves the floating window as-is (it already has the tab).
    /// </summary>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);

        if (_floatingState is not { } fs)
            return;

        _floatingState = null;
        fs.FloatingWindow.Opacity = 1.0;
    }

    /// <summary>
    /// Accepts a merged tab item and continues dragging it within this <see cref="TabsControl"/>.
    /// The pointer is captured to this control, and <see cref="OnPointerMoved"/> /
    /// <see cref="OnPointerReleased"/> handle the ongoing drag (including re-tear-off via MonitorBreach).
    /// </summary>
    /// <param name="item"></param>
    /// <param name="pointer"></param>
    /// <param name="cursorScreen"></param>
    /// <param name="pointerInTab">The pointer offset inside the tab at drag start (DIPs), from the source.</param>
    internal void ReceiveMergedItemDrag(object item, IPointer pointer, PixelPoint cursorScreen, Point pointerInTab)
    {
        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0;

        // Use the original pointer-in-tab offset from the source so the
        // relative cursor-to-tab position stays consistent across merges.
        _dragStartPointerInTab = pointerInTab;

        // Calculate cursor X relative to the TabsPanel to determine the insert index.
        double cursorXInPanel = 0;
        try
        {
            var panelScreen = _tabsPanel.PointToScreen(new Point(0, 0));
            cursorXInPanel = (cursorScreen.X - panelScreen.X) / scaling;
        }
        catch
        {
            // Fall through — insert at end
        }

        // Find the best insertion index based on cursor position among existing tabs.
        int insertIndex = CalculateInsertIndex(cursorXInPanel);

        // Insert the item at the computed position (instead of appending at end)
        if (insertIndex >= Items.Count)
            Items.Add(item);
        else
            Items.Insert(insertIndex, item);

        SelectedItem = item;

        // Force a synchronous layout pass so that TabsPanel recalculates _itemWidth
        // for the new tab count. Without this, the tab widths reflect the old count
        // and overflow under the window control buttons.
        InvalidateTabLayout();
        UpdateLayout();

        // Now find the container after layout is complete
        if (ContainerFromItem(item) is not DragTabItem container)
            return;

        // Compute the X position of the tab so it appears under the cursor at the grab point
        try
        {
            var panelScreen = _tabsPanel.PointToScreen(new Point(0, 0));
            container.X = ((cursorScreen.X - panelScreen.X) / scaling) - _dragStartPointerInTab.X;
        }
        catch
        {
            // Leave X at default (0) if we can't compute
        }

        // Set up drag state on this TabsControl
        _draggedItem = container;
        _dragging = true;
        container.IsSelected = true;

        // Set drag visual state
        foreach (var di in DragTabItems())
        {
            di.IsDragging = false;
            di.IsSiblingDragging = true;
        }
        container.IsDragging = true;
        container.IsSiblingDragging = false;

        // Capture pointer to this TabsControl so OnPointerMoved/Released drive the drag.
        // This makes the merged tab follow the cursor and enables re-tear-off.
        pointer.Capture(this);

        Dispatcher.UIThread.Post(() => _tabsPanel.InvalidateMeasure(), DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Calculates the insertion index for a tab being merged, based on the cursor's
    /// horizontal position within the <see cref="TabsPanel"/>.
    /// </summary>
    private int CalculateInsertIndex(double cursorXInPanel)
    {
        int index = 0;
        foreach (var di in DragTabItems())
        {
            var tabMid = di.X + (di.Bounds.Width / 2);
            if (cursorXInPanel > tabMid)
                index = di.LogicalIndex + 1;
        }
        return index;
    }

    /// <summary>
    /// Finds a <see cref="TabsControl"/> instance (in any window) whose tab header area
    /// contains the given screen point and belongs to the same partition.
    /// </summary>
    /// <param name="screenPoint">Pointer position in screen coordinates.</param>
    /// <param name="partition">Partition string to match (null matches null).</param>
    /// <param name="exclude">A <see cref="TabsControl"/> to exclude (e.g. the floating window's TC).</param>
    private static TabsControl? FindReentryTarget(PixelPoint screenPoint, string? partition, TabsControl? exclude)
    {
        foreach (var tc in s_loadedInstances)
        {
            if (tc == exclude)
                continue;

            // Partition matching: both must be null, or the same string
            var tcPartition = tc.InterTabController?.Partition;
            if (tcPartition != partition)
                continue;

            if (tc.IsPointInTabHeaderArea(screenPoint))
                return tc;
        }

        return null;
    }

    /// <summary>
    /// Handles the scenario where the source <see cref="TabsControl"/> becomes empty after a tab transfer.
    /// </summary>
    private void HandleSourceEmptied(int removedIndex)
    {
        if (Items.Count > 0)
        {
            if (removedIndex >= 0 && removedIndex < Items.Count)
                SelectedItem = Items[removedIndex];
            else if (Items.Count > 0)
                SelectedItem = Items[^1];
            return;
        }

        if (TopLevel.GetTopLevel(this) is not Window window)
            return;

        Dispatcher.UIThread.Post(() => window.Close());
    }

    /// <summary>
    /// Forces the TabsPanel and its TopPanel ancestor to re-measure and re-arrange.
    /// Called after inter-tab item operations to ensure correct tab widths.
    /// </summary>
    private void InvalidateTabLayout()
    {
        _tabsPanel.InvalidateMeasure();
        if (_tabsPanel.Parent is Layoutable presenter)
        {
            presenter.InvalidateMeasure();
            if (presenter.Parent is Layoutable topPanel)
                topPanel.InvalidateMeasure();
        }
    }

    /// <summary>
    /// Mutable state held on the SOURCE <see cref="TabsControl"/> during a tear-off drag.
    /// The source retains pointer capture and uses this state to move the floating window
    /// and detect merge targets.
    /// </summary>
    private sealed class FloatingDragState
    {
        /// <summary>The floating window created by the tear-off, or the source window for single-tab drag.</summary>
        public required Window FloatingWindow { get; init; }

        /// <summary>The <see cref="TabsControl"/> inside the floating window, or this TC for single-tab drag.</summary>
        public required TabsControl FloatingTabsControl { get; init; }

        /// <summary>
        /// Pixel offset from the pointer to the floating window's top-left corner,
        /// preserved so the window follows the pointer smoothly.
        /// </summary>
        public required Point DragOffset { get; init; }

        /// <summary>
        /// True when this is a single-tab window drag (no tear-off occurred).
        /// The source window itself is being moved for merge purposes.
        /// </summary>
        public bool IsSelfDrag { get; init; }
    }

    #endregion
}
