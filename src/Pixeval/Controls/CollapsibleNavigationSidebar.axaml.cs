using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Pixeval.Controls;

public class CollapsibleNavigationSidebar : ItemsControl
{
    private Button _moreButton;
    private Border _moreButtonHost;
    private MenuFlyout _moreFlyout;
    private readonly List<Control> _itemContainers = new();

    public static readonly StyledProperty<IEnumerable> HeaderItemsProperty =
        AvaloniaProperty.Register<CollapsibleNavigationSidebar, IEnumerable>(nameof(HeaderItems));

    public static readonly StyledProperty<IEnumerable> FooterItemsProperty =
        AvaloniaProperty.Register<CollapsibleNavigationSidebar, IEnumerable>(nameof(FooterItems));

    public IEnumerable HeaderItems
    {
        get => GetValue(HeaderItemsProperty);
        set => SetValue(HeaderItemsProperty, value);
    }

    public IEnumerable FooterItems
    {
        get => GetValue(FooterItemsProperty);
        set => SetValue(FooterItemsProperty, value);
    }

    static CollapsibleNavigationSidebar()
    {
        HeaderItemsProperty.Changed.AddClassHandler<CollapsibleNavigationSidebar>((s, e) => s.UpdateItems());
        FooterItemsProperty.Changed.AddClassHandler<CollapsibleNavigationSidebar>((s, e) => s.UpdateItems());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _moreButtonHost = e.NameScope.Get<Border>("PART_MoreButtonHost");
        _moreButton = e.NameScope.Get<Button>("PART_MoreButton");
        _moreFlyout = new MenuFlyout { Placement = PlacementMode.TopEdgeAlignedLeft };
        _moreButton.Flyout = _moreFlyout;
        _moreButton.Click += OnMoreButtonClick;
        _moreButtonHost.IsVisible = false;
        this.SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateVisibility(e.NewSize.Height);
    }

    private void UpdateItems()
    {
        var combined = new List<object>();
        if (HeaderItems != null) combined.AddRange(HeaderItems.Cast<object>());
        if (FooterItems != null) combined.AddRange(FooterItems.Cast<object>());
        ItemsSource = combined;
        _itemContainers.Clear();
        InvalidateMeasure();
        Dispatcher.UIThread.Post(() => UpdateVisibility(Bounds.Height), DispatcherPriority.Loaded);
    }

    private void UpdateVisibility(double availableHeight)
    {
        if (Items.Count == 0 || availableHeight <= 0) return;

        var containers = new List<Control>();
        for (int i = 0; i < Items.Count; i++)
        {
            if (this.ContainerFromIndex(i) is Control c)
                containers.Add(c);
        }

        if (containers.Count != _itemContainers.Count)
        {
            _itemContainers.Clear();
            _itemContainers.AddRange(containers);
        }

        if (_itemContainers.Count == 0) return;

        double reservedHeight = 50;
        double totalHeight = 0;
        int visibleCount = 0;

        foreach (var container in _itemContainers)
        {
            if (container.DesiredSize.Height <= 0)
                container.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        }

        for (int i = 0; i < _itemContainers.Count; i++)
        {
            double h = _itemContainers[i].DesiredSize.Height > 0 ? _itemContainers[i].DesiredSize.Height : 40;
            if (totalHeight + h + reservedHeight <= availableHeight)
            {
                totalHeight += h;
                visibleCount++;
            }
            else break;
        }

        bool hasMore = visibleCount < _itemContainers.Count;

        for (int i = 0; i < _itemContainers.Count; i++)
            _itemContainers[i].IsVisible = i < visibleCount;

        _moreButtonHost.IsVisible = hasMore;
        _moreFlyout.ItemsSource = hasMore ? _itemContainers.Skip(visibleCount).Select(c => c.DataContext).ToList() : null;
    }

    private void OnMoreButtonClick(object sender, RoutedEventArgs e)
    {
        _moreFlyout.ShowAt(_moreButton);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == ItemsSourceProperty && change.NewValue != null)
            Dispatcher.UIThread.Post(() => UpdateVisibility(Bounds.Height), DispatcherPriority.Loaded);
        base.OnPropertyChanged(change);
    }
}
