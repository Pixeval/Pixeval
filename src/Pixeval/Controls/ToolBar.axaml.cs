using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using Avalonia.Threading;

namespace Pixeval.Controls;

/// <summary>
/// A toolbar control similar to WinUI's CommandBar.
/// Items that exceed the available width are automatically moved into an overflow popup
/// accessible via a "More" button (three dots).
/// </summary>
/// <remarks>
/// Place <see cref="Button"/> or any <see cref="Control"/> elements directly as children:
/// <code>
/// &lt;local:ToolBar&gt;
///     &lt;Button Content="Copy" /&gt;
///     &lt;Button Content="Paste" /&gt;
/// &lt;/local:ToolBar&gt;
/// </code>
/// </remarks>
public class ToolBar : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="Spacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<ToolBar, double>(nameof(Spacing), 2);

    /// <summary>
    /// Defines the <see cref="OverflowSpacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> OverflowSpacingProperty =
        AvaloniaProperty.Register<ToolBar, double>(nameof(OverflowSpacing), 2);

    /// <summary>
    /// Defines the <see cref="OverflowPlacement"/> property.
    /// </summary>
    public static readonly StyledProperty<PlacementMode> OverflowPlacementProperty =
        AvaloniaProperty.Register<ToolBar, PlacementMode>(nameof(OverflowPlacement), PlacementMode.BottomEdgeAlignedRight);

    /// <summary>
    /// Defines the <see cref="HasOverflow"/> property.
    /// </summary>
    public static readonly DirectProperty<ToolBar, bool> HasOverflowProperty =
        AvaloniaProperty.RegisterDirect<ToolBar, bool>(nameof(HasOverflow), o => o.HasOverflow);

    private Panel? _primaryHost;
    private Panel? _overflowHost;
    private Button? _overflowButton;
    private Popup? _overflowPopup;
    private bool _isUpdating;
    private bool _updateQueued;
    private readonly List<Control> _subscribedItems = [];

    public ToolBar()
    {
        Items.CollectionChanged += OnItemsCollectionChanged;
    }

    /// <summary>
    /// Gets the collection of items displayed in the toolbar.
    /// Place <see cref="Button"/> or other <see cref="Control"/> elements here.
    /// </summary>
    [Content]
    public AvaloniaList<Control> Items { get; } = [];

    /// <summary>
    /// Gets or sets the spacing between items in the primary area.
    /// </summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between items in the overflow popup panel.
    /// </summary>
    public double OverflowSpacing
    {
        get => GetValue(OverflowSpacingProperty);
        set => SetValue(OverflowSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the <see cref="PlacementMode"/> for the overflow popup.
    /// Default is <see cref="PlacementMode.BottomEdgeAlignedRight"/>.
    /// </summary>
    public PlacementMode OverflowPlacement
    {
        get => GetValue(OverflowPlacementProperty);
        set => SetValue(OverflowPlacementProperty, value);
    }

    /// <summary>
    /// Gets a value indicating whether any items have overflowed into the popup.
    /// </summary>
    public bool HasOverflow
    {
        get;
        private set => SetAndRaise(HasOverflowProperty, ref field, value);
    }

    /// <summary>
    /// Forces a recalculation of the overflow state.
    /// Call this if the content of items changes (e.g., button text updated)
    /// and the toolbar should re-evaluate which items fit.
    /// </summary>
    public void InvalidateOverflow() => QueueOverflowUpdate();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_overflowButton is not null)
            _overflowButton.Click -= OnOverflowButtonClick;

        base.OnApplyTemplate(e);

        _primaryHost = e.NameScope.Find<Panel>("PART_PrimaryHost");
        _overflowHost = e.NameScope.Find<Panel>("PART_OverflowHost");
        _overflowButton = e.NameScope.Find<Button>("PART_OverflowButton");
        _overflowPopup = e.NameScope.Find<Popup>("PART_OverflowPopup");

        if (_overflowButton is not null)
            _overflowButton.Click += OnOverflowButtonClick;

        SyncAllItems();
        QueueOverflowUpdate();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        QueueOverflowUpdate();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SpacingProperty ||
            change.Property == PaddingProperty ||
            change.Property == BorderThicknessProperty)
        {
            QueueOverflowUpdate();
        }
    }

    private void OnOverflowButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _overflowPopup?.IsOpen = !_overflowPopup.IsOpen;
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SyncAllItems();
        QueueOverflowUpdate();
    }

    private void QueueOverflowUpdate()
    {
        if (_updateQueued)
            return;
        _updateQueued = true;
        Dispatcher.UIThread.Post(() =>
        {
            _updateQueued = false;
            UpdateOverflow();
        }, DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Moves all items back into the primary host panel (re-syncs from <see cref="Items"/>).
    /// Also manages subscriptions to child <see cref="Visual.IsVisibleProperty"/> changes.
    /// </summary>
    private void SyncAllItems()
    {
        if (_primaryHost is null)
            return;

        foreach (var item in _subscribedItems)
            item.PropertyChanged -= OnItemPropertyChanged;
        _subscribedItems.Clear();

        _overflowHost?.Children.Clear();
        _primaryHost.Children.Clear();

        foreach (var item in Items)
        {
            _primaryHost.Children.Add(item);
            item.PropertyChanged += OnItemPropertyChanged;
            _subscribedItems.Add(item);
        }
    }

    private void OnItemPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == IsVisibleProperty)
            QueueOverflowUpdate();
    }

    /// <summary>
    /// Core overflow logic: measures items, determines which fit,
    /// and redistributes between primary host and overflow popup.
    /// </summary>
    private void UpdateOverflow()
    {
        if (_isUpdating || _primaryHost is null || _overflowButton is null || _overflowHost is null)
            return;

        _isUpdating = true;
        try
        {
            _overflowPopup?.IsOpen = false;

            var items = Items.ToList();

            // Move all items to primary host so we can measure them in-tree
            _overflowHost.Children.Clear();
            _primaryHost.Children.Clear();

            if (items.Count == 0)
            {
                HasOverflow = false;
                _overflowButton.IsVisible = false;
                return;
            }

            foreach (var item in items)
                _primaryHost.Children.Add(item);

            // Measure each item with infinite available space
            var widths = new double[items.Count];
            var totalWidth = 0d;
            var spacing = Spacing;
            var visibleCount = 0;

            for (var i = 0; i < items.Count; i++)
            {
                if (!items[i].IsVisible)
                {
                    widths[i] = 0;
                    continue;
                }

                items[i].Measure(Size.Infinity);
                widths[i] = items[i].DesiredSize.Width;
                totalWidth += widths[i] + (visibleCount > 0 ? spacing : 0);
                visibleCount++;
            }

            // Compute the usable width inside the border and padding
            var available = Bounds.Width
                            - Padding.Left - Padding.Right
                            - BorderThickness.Left - BorderThickness.Right;

            if (available <= 0 || totalWidth <= available)
            {
                // Everything fits — no overflow
                HasOverflow = false;
                _overflowButton.IsVisible = false;
                return;
            }

            // Need overflow — reserve space for the "..." button
            _overflowButton.IsVisible = true;
            _overflowButton.Measure(Size.Infinity);
            var maxPrimary = available - _overflowButton.DesiredSize.Width - spacing;

            var overflowStart = items.Count;
            var used = 0d;
            var usedVisibleCount = 0;

            for (var i = 0; i < items.Count; i++)
            {
                if (!items[i].IsVisible)
                    continue;

                var space = widths[i] + (usedVisibleCount > 0 ? spacing : 0);
                if (used + space > maxPrimary)
                {
                    overflowStart = i;
                    break;
                }

                used += space;
                usedVisibleCount++;
            }

            // Move overflowed items from primary → overflow host
            for (var i = items.Count - 1; i >= overflowStart; i--)
                _primaryHost.Children.RemoveAt(i);

            for (var i = overflowStart; i < items.Count; i++)
                _overflowHost.Children.Add(items[i]);

            HasOverflow = overflowStart < items.Count;
        }
        finally
        {
            _isUpdating = false;
        }
    }
}
