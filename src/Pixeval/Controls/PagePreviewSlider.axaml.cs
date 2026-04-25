using System;
using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;

namespace Pixeval.Controls;

public class PagePreviewSlider : TemplatedControl
{
    public static readonly StyledProperty<IList?> ItemsSourceProperty =
        AvaloniaProperty.Register<PagePreviewSlider, IList?>(nameof(ItemsSource));

    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<PagePreviewSlider, int>(
            nameof(SelectedIndex),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<PagePreviewSlider, IDataTemplate?>(nameof(ItemTemplate));

    public static readonly DirectProperty<PagePreviewSlider, string> IndicatorTextProperty =
        AvaloniaProperty.RegisterDirect<PagePreviewSlider, string>(
            nameof(IndicatorText),
            o => o.IndicatorText);

    public static readonly DirectProperty<PagePreviewSlider, int> PreviewIndexProperty =
        AvaloniaProperty.RegisterDirect<PagePreviewSlider, int>(
            nameof(PreviewIndex),
            o => o.PreviewIndex);

    public static readonly DirectProperty<PagePreviewSlider, object?> PreviewItemProperty =
        AvaloniaProperty.RegisterDirect<PagePreviewSlider, object?>(
            nameof(PreviewItem),
            o => o.PreviewItem);

    private IList? _itemsSource;
    private INotifyCollectionChanged? _notifyCollectionChanged;
    private Border? _track;
    private Border? _selectedTrack;
    private Border? _thumb;
    private Canvas? _thumbCanvas;
    private Popup? _previewPopup;
    private TextBlock? _previewLabel;
    private ContentControl? _previewContent;
    private bool _isDragging;
    private bool _isHovering;
    private double _previewPointerX;

    public IList? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public string IndicatorText
    {
        get;
        private set => SetAndRaise(IndicatorTextProperty, ref field, value);
    } = string.Empty;

    public int PreviewIndex
    {
        get;
        private set => SetAndRaise(PreviewIndexProperty, ref field, value);
    } = -1;

    public object? PreviewItem
    {
        get;
        private set => SetAndRaise(PreviewItemProperty, ref field, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachTemplateHandlers();

        base.OnApplyTemplate(e);

        _track = e.NameScope.Find<Border>("PART_Track");
        _selectedTrack = e.NameScope.Find<Border>("PART_SelectedTrack");
        _thumb = e.NameScope.Find<Border>("PART_Thumb");
        _thumbCanvas = e.NameScope.Find<Canvas>("PART_ThumbCanvas");
        _previewPopup = e.NameScope.Find<Popup>("PART_PreviewPopup");
        _previewLabel = e.NameScope.Find<TextBlock>("PART_PreviewLabel");
        _previewContent = e.NameScope.Find<ContentControl>("PART_PreviewContent");

        if (_track is not null)
            _track.SizeChanged += TemplatePartOnSizeChanged;

        if (_thumb is not null)
            _thumb.SizeChanged += TemplatePartOnSizeChanged;

        SynchronizeState();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        if (_isDragging)
            return;

        UpdatePreviewFromPointer(e, commitSelection: false);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        UpdatePreviewFromPointer(e, commitSelection: _isDragging);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        if (_isDragging)
            return;

        ClearPreview();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (ItemsCount <= 1 ||
            e.GetCurrentPoint(this).Properties.IsLeftButtonPressed is false)
        {
            return;
        }

        _isDragging = true;
        e.Pointer.Capture(this);
        UpdatePreviewFromPointer(e, commitSelection: true);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!_isDragging)
            return;

        UpdatePreviewFromPointer(e, commitSelection: true);
        e.Pointer.Capture(null);
        _isDragging = false;

        if (!IsPointerOver)
            ClearPreview();

        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _isDragging = false;

        if (!IsPointerOver)
            ClearPreview();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
        {
            if (!ReferenceEquals(_itemsSource, change.OldValue))
                UnsubscribeFromItemsSource();

            _itemsSource = change.GetNewValue<IList?>();
            SubscribeToItemsSource();
            SynchronizeState();
        }
        else if (change.Property == SelectedIndexProperty)
        {
            SynchronizeState();
        }
        else if (change.Property == ItemTemplateProperty)
        {
            UpdatePreviewPopupState();
        }
    }

    private int ItemsCount => ItemsSource?.Count ?? 0;

    private void SubscribeToItemsSource()
    {
        if (_itemsSource is INotifyCollectionChanged notifyCollectionChanged)
        {
            _notifyCollectionChanged = notifyCollectionChanged;
            _notifyCollectionChanged.CollectionChanged += ItemsSourceOnCollectionChanged;
        }
    }

    private void UnsubscribeFromItemsSource()
    {
        if (_notifyCollectionChanged is null)
            return;

        _notifyCollectionChanged.CollectionChanged -= ItemsSourceOnCollectionChanged;
        _notifyCollectionChanged = null;
    }

    private void ItemsSourceOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => SynchronizeState();

    private void TemplatePartOnSizeChanged(object? sender, SizeChangedEventArgs e)
        => UpdateVisualState();

    private void DetachTemplateHandlers()
    {
        if (_track is not null)
            _track.SizeChanged -= TemplatePartOnSizeChanged;

        if (_thumb is not null)
            _thumb.SizeChanged -= TemplatePartOnSizeChanged;

        _track = null;
        _selectedTrack = null;
        _thumb = null;
        _thumbCanvas = null;
        _previewPopup = null;
        _previewLabel = null;
        _previewContent = null;
    }

    private void SynchronizeState()
    {
        SetCurrentValue(IsVisibleProperty, ItemsCount > 1);

        if (ItemsCount <= 0)
        {
            IndicatorText = string.Empty;
            PreviewIndex = -1;
            PreviewItem = null;
            _previewPointerX = 0;
            UpdatePreviewPopupState();
            UpdateVisualState();
            return;
        }

        var coercedSelectedIndex = CoerceIndex(SelectedIndex);
        if (coercedSelectedIndex != SelectedIndex)
        {
            SetCurrentValue(SelectedIndexProperty, coercedSelectedIndex);
            return;
        }

        if (_isHovering)
        {
            var coercedPreviewIndex = CoerceIndex(PreviewIndex >= 0 ? PreviewIndex : SelectedIndex);
            PreviewIndex = coercedPreviewIndex;
            PreviewItem = GetItem(coercedPreviewIndex);
        }
        else
        {
            PreviewIndex = -1;
            PreviewItem = null;
        }

        IndicatorText = BuildIndicatorText();
        UpdatePreviewPopupState();
        UpdateVisualState();
    }

    private void UpdatePreviewFromPointer(PointerEventArgs e, bool commitSelection)
    {
        if (ItemsCount <= 1 || _track is null)
            return;

        var point = e.GetPosition(_track);
        _previewPointerX = Clamp(point.X, 0, _track.Bounds.Width);
        var index = GetIndexFromTrackX(_previewPointerX);
        _isHovering = true;
        PreviewIndex = index;
        PreviewItem = GetItem(index);

        if (commitSelection && SelectedIndex != index)
            SetCurrentValue(SelectedIndexProperty, index);

        IndicatorText = BuildIndicatorText();
        UpdatePreviewPopupState();
        UpdateVisualState();
    }

    private void ClearPreview()
    {
        _isHovering = false;
        PreviewIndex = -1;
        PreviewItem = null;
        IndicatorText = BuildIndicatorText();
        UpdatePreviewPopupState();
        UpdateVisualState();
    }

    private void UpdatePreviewPopupState()
    {
        if (_previewPopup is null)
            return;

        if (_previewLabel is not null)
            _previewLabel.Text = IndicatorText;

        if (_previewContent is not null)
        {
            _previewContent.Content = PreviewItem;
            _previewContent.ContentTemplate = ItemTemplate;
            _previewContent.IsVisible = PreviewItem is not null && ItemTemplate is not null;
        }

        _previewPopup.IsOpen = _isHovering && ItemsCount > 1;

        if (_previewPopup.IsOpen)
            UpdatePreviewPopupPosition();
    }

    private void UpdatePreviewPopupPosition()
    {
        if (_previewPopup is null || _track is null)
            return;

        var trackWidth = _track.Bounds.Width;
        if (trackWidth <= 0)
            return;

        var popupX = _isHovering
            ? _previewPointerX
            : GetCenterX(CoerceIndex(SelectedIndex), trackWidth);
        _previewPopup.HorizontalOffset = popupX - trackWidth / 2d;
    }

    private void UpdateVisualState()
    {
        if (_track is null || _thumb is null || _thumbCanvas is null)
            return;

        var trackWidth = _track.Bounds.Width;
        if (trackWidth <= 0 || ItemsCount <= 0)
            return;

        var activeIndex = _isHovering && PreviewIndex >= 0
            ? PreviewIndex
            : SelectedIndex;
        var center = GetCenterX(CoerceIndex(activeIndex), trackWidth);

        _selectedTrack?.Width = Math.Clamp(center, 0, trackWidth);

        var thumbLeft = center - _thumb.Bounds.Width / 2d;
        Canvas.SetLeft(_thumb, Clamp(thumbLeft, 0, Math.Max(0, _thumbCanvas.Bounds.Width - _thumb.Bounds.Width)));

        UpdatePreviewPopupPosition();
    }

    private double GetCenterX(int index, double trackWidth)
    {
        if (ItemsCount <= 1)
            return trackWidth / 2d;

        return trackWidth * index / (ItemsCount - 1);
    }

    private int GetIndexFromTrackX(double trackX)
    {
        if (ItemsCount <= 1 || _track is null || _track.Bounds.Width <= 0)
            return 0;

        var normalized = Clamp(trackX / _track.Bounds.Width, 0, 1);
        return CoerceIndex((int)Math.Round(normalized * (ItemsCount - 1), MidpointRounding.AwayFromZero));
    }

    private int CoerceIndex(int index)
    {
        if (ItemsCount <= 0)
            return 0;

        return Math.Clamp(index, 0, ItemsCount - 1);
    }

    private object? GetItem(int index)
    {
        if (ItemsSource is null || index < 0 || index >= ItemsCount)
            return null;

        return ItemsSource[index];
    }

    private string BuildIndicatorText()
    {
        if (ItemsCount <= 0)
            return string.Empty;

        var activeIndex = _isHovering && PreviewIndex >= 0
            ? PreviewIndex
            : CoerceIndex(SelectedIndex);
        return $"{activeIndex + 1}/{ItemsCount}";
    }

    private static double Clamp(double value, double min, double max)
        => value < min ? min : value > max ? max : value;
}
