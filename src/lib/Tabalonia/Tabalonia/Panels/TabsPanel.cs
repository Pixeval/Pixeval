using Tabalonia.Controls;


namespace Tabalonia.Panels;


public class TabsPanel : Panel
{
    #region Private Fields
    
    private readonly TabsControl _tabsControl;
    
    private readonly Dictionary<DragTabItem, LocationInfo> _itemsLocations = new();
    private double _itemWidth;
    private readonly Dictionary<DragTabItem, double> _activeStoryboardTargetLocations = new();
    private DragTabItem? _dragItem;
    
    #endregion


    public event Action? DragCompleted;
    
    
    public TabsPanel(TabsControl tabsControl) => _tabsControl = tabsControl;


    #region Public Properties

    public double ItemWidth { get; internal set; }

    public double ItemOffset { get; internal set; }
    
    #endregion

    
    #region Protected Methods
    
    protected override Size MeasureOverride(Size availableSize)
    {
        var draggedItem = GetDragItem();

        return draggedItem is not null 
            ? DragMeasureImpl(draggedItem, availableSize) 
            : MeasureImpl(availableSize);
    }
        

    protected override Size ArrangeOverride(Size finalSize)
    {
        var draggedItem = GetDragItem();
            
        if (_dragItem is not null && draggedItem is null)
        {
            var oldDragItem = _dragItem;
            _dragItem = null;
                
            return DragCompletedArrangeImpl(oldDragItem, finalSize);
        }
            
        _dragItem = draggedItem;
            
        return draggedItem is not null
            ? DragArrangeImpl(draggedItem, finalSize) 
            : ArrangeImpl(finalSize);
    }
        
    #endregion
    
    
    private Size MeasureImpl(Size availableSize)
    {
        _itemWidth = GetAvailableWidth(availableSize);

        double height = 0;
        double width = 0;

        bool isFirst = true;

        foreach (var tabItem in Children)
        {
            tabItem.Measure(new Size(_itemWidth, availableSize.Height));

            width += _itemWidth;
            height = double.Max(tabItem.DesiredSize.Height, height);

            if (!isFirst)
                width += ItemOffset;

            isFirst = false;
        }

        return new Size(width, height);
    }


    private Size DragMeasureImpl(DragTabItem draggedItem, Size availableSize)
    {
        double height = 0;
        double width = 0;

        bool isFirst = true;

        foreach (var tabItem in Children)
        {
            tabItem.Measure(new Size(_itemWidth, availableSize.Height));
                
            width += _itemWidth;
            height = double.Max(tabItem.DesiredSize.Height, height);

            if (!isFirst)
                width += ItemOffset;

            isFirst = false;
        }
            
        if (draggedItem.X + _itemWidth > width)
            return new Size(draggedItem.X + _itemWidth, height);
            
        return new Size(width, height);
    }
        
        
    private Size ArrangeImpl(Size finalSize)
    {
        double x = 0;
        int z = ZIndexes.NonSelected;
        int logicalIndex = 0;

        _itemsLocations.Clear();
            
        foreach (var child in Children)
        {
            if (child is not DragTabItem tabItem)
                continue;

            tabItem.ZIndex = tabItem.IsSelected ? int.MaxValue : --z;
            tabItem.LogicalIndex = logicalIndex++;
            
            SetLocation(tabItem, x, _itemWidth);
                
            _itemsLocations.Add(tabItem, GetLocationInfo(tabItem));

            x += _itemWidth + ItemOffset;
        }

        return finalSize;
    }

        
    private Size DragArrangeImpl(DragTabItem dragItem, Size finalSize)
    {
        var dragItemsLocations = GetLocations(Children.OfType<DragTabItem>(), dragItem);
        
        double currentCoord = 0.0;
            
            
        foreach (var location in dragItemsLocations)
        {
            var item = location.Item;

            if (!Equals(item, dragItem) && item.LogicalIndex >= _tabsControl.FixedHeaderCount)
            {
                SendToLocation(item, currentCoord, _itemWidth);
            }
            else
            {
                double maxX = finalSize.Width - _itemWidth;

                if (dragItem.X > maxX) dragItem.X = maxX;

                double minX = CalculateMinX();

                if (dragItem.X < minX) dragItem.X = minX;

                SetLocation(dragItem, dragItem.X, _itemWidth);
            }
                
            currentCoord += _itemWidth + ItemOffset;
        }
        
        return finalSize;
    }
    

    private double CalculateMinX()
    {
        if (_tabsControl.FixedHeaderCount < 1)
            return 0;
        
        double x = 0;

        for (int index = 0; index < _tabsControl.FixedHeaderCount; index++)
        {
            x += _itemWidth + ItemOffset;
        }

        return x;
    }


    private Size DragCompletedArrangeImpl(DragTabItem dragItem, Size finalSize)
    {
        var dragItemsLocations = GetLocations(Children.OfType<DragTabItem>(), dragItem);
            
        double currentCoord = 0.0;
        int z = ZIndexes.NonSelected;
        int logicalIndex = 0;

        foreach (var location in dragItemsLocations)
        {
            var item = location.Item;

            SetLocation(item, currentCoord, _itemWidth);
            currentCoord += _itemWidth + ItemOffset;
            item.ZIndex = --z;
            item.LogicalIndex = logicalIndex++;
        }

        dragItem.ZIndex = ZIndexes.Selected;
        
        DragCompleted?.Invoke();
            
        return finalSize;
    }
        

    private double GetAvailableWidth(Size availableSize)
    {
        int tabsCount = Children.Count;

        if (tabsCount == 0)
            return 0;

        double itemWidth = availableSize.Width / tabsCount - ItemOffset * (tabsCount - 1) / tabsCount;

        return double.Min(ItemWidth, itemWidth);
    }

        
    private IEnumerable<LocationInfo> GetLocations(IEnumerable<DragTabItem> allItems, DragTabItem dragItem)
    {
        double OrderSelector(LocationInfo loc)
        {
            if (Equals(loc.Item, dragItem))
            {
                if (!_itemsLocations.TryGetValue(dragItem, out var dragItemInfo))
                    return loc.Start;
                    
                return loc.Start > dragItemInfo.Start ? loc.End : loc.Start;
            }

            return _itemsLocations.TryGetValue(loc.Item, out var itemInfo) ? itemInfo.Mid : loc.Start;
        }

        var currentLocations = allItems
            .Select(GetLocationInfo)
            .OrderBy(OrderSelector);

        return currentLocations;
    }
        
        
    private async void SendToLocation(DragTabItem item, double location, double width)
    {
        bool itemIsAnimating = _activeStoryboardTargetLocations.TryGetValue(item, out double activeTarget);
        
        if (itemIsAnimating)
        {
            SetLocation(item, item.X, width);
            return;
        }
        
        if (double.Abs(item.X - location) < 1.0 || itemIsAnimating && double.Abs(activeTarget - location) < 1.0)
        {
            return;
        }
        
        _activeStoryboardTargetLocations[item] = location;

        const int animDuration = 200;

        var animation = new Animation
        {
            Easing = new CubicEaseOut(),
            Duration = TimeSpan.FromMilliseconds(animDuration),
            PlaybackDirection = PlaybackDirection.Normal,
            FillMode = FillMode.None,
            Children =
            {
                new KeyFrame
                {
                    KeyTime = TimeSpan.FromMilliseconds(animDuration),
                    Setters =
                    {
                        new Setter(DragTabItem.XProperty, location),
                    }
                }
            }
        };
            
        await animation.RunAsync(item);

        SetLocation(item, location, width);
            
        _activeStoryboardTargetLocations.Remove(item);
    }


    private static void SetLocation(DragTabItem dragTabItem, double x, double width)
    {
        const double y = 0;

        dragTabItem.X = x;
        dragTabItem.Y = y;

        dragTabItem.Arrange(new Rect(new Point(x, y), new Size(width, dragTabItem.DesiredSize.Height)));
    }
        
        
    private LocationInfo GetLocationInfo(DragTabItem item)
    {
        double size = item.Bounds.Width;
            
        if (!_activeStoryboardTargetLocations.TryGetValue(item, out var startLocation))
            startLocation = item.X;
            
        double midLocation = startLocation + size / 2;
        double endLocation = startLocation + size;

        return new LocationInfo(item, startLocation, midLocation, endLocation);
    }


    private DragTabItem? GetDragItem() => (DragTabItem?)Children.FirstOrDefault(c => c is DragTabItem
    {
        IsDragging: true
    });


    #region Private Structs

    private readonly record struct LocationInfo(DragTabItem Item, double Start, double Mid, double End);

    #endregion
}
