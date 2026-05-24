// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using FluentIcons.Common;
using Pixeval.I18N;
using Pixeval.Models.Home;

namespace Pixeval.Views.Home;

public sealed partial class HomePageCardControl : TemplatedControl
{
    private const string PcEditing = ":editing";
    private const string PcSelected = ":selected";

    public static readonly DirectProperty<HomePageCardControl, HomePageCardLayout> CardProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, HomePageCardLayout>(nameof(Card), control => control.Card, (control, value) => control.Card = value);

    public static readonly DirectProperty<HomePageCardControl, HomeCardTemplate> CardTemplateProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, HomeCardTemplate>(nameof(CardTemplate), control => control.CardTemplate,
            (control, value) => control.CardTemplate = value);

    public static readonly DirectProperty<HomePageCardControl, int> RowCountProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, int>(nameof(RowCount), control => control.RowCount, (control, value) => control.RowCount = value);

    public static readonly DirectProperty<HomePageCardControl, int> ColumnCountProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, int>(nameof(ColumnCount), control => control.ColumnCount, (control, value) => control.ColumnCount = value);

    public static readonly StyledProperty<bool> IsEditingProperty =
        AvaloniaProperty.Register<HomePageCardControl, bool>(nameof(IsEditing));

    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<HomePageCardControl, bool>(nameof(IsSelected));

    public static readonly DirectProperty<HomePageCardControl, string> CardTitleProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, string>(nameof(CardTitle), control => control.CardTitle,
            (control, value) => control.CardTitle = value);

    public static readonly DirectProperty<HomePageCardControl, Symbol> CardSymbolProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, Symbol>(nameof(CardSymbol), control => control.CardSymbol,
            (control, value) => control.CardSymbol = value);

    public static readonly DirectProperty<HomePageCardControl, string?> PlaceholderTextProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, string?>(nameof(PlaceholderText), control => control.PlaceholderText,
            (control, value) => control.PlaceholderText = value);

    public static readonly DirectProperty<HomePageCardControl, object?> PreviewContentProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, object?>(nameof(PreviewContent), control => control.PreviewContent,
            (control, value) => control.PreviewContent = value);

    public static readonly DirectProperty<HomePageCardControl, bool> IsPreviewVisibleProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, bool>(nameof(IsPreviewVisible), control => control.IsPreviewVisible);

    public static readonly DirectProperty<HomePageCardControl, bool> IsPlaceholderVisibleProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, bool>(nameof(IsPlaceholderVisible), control => control.IsPlaceholderVisible);

    private static readonly HomeCardTemplate _PlaceholderTemplate = new(
        HomePageCardSourceKind.WorkRecommended,
        HomePageCardTemplateKind.WorkList,
        "",
        "",
        1,
        1,
        default);

    private Grid? _rootGrid;
    private Grid? _resizeHandlesLayer;
    private PointerEditState? _pointerEditState;

    public HomePageCardControl(
        HomePageCardLayout card,
        HomeCardTemplate template,
        int rowCount,
        int columnCount)
    {
        Card = card;
        CardTemplate = template;
        RowCount = rowCount;
        ColumnCount = columnCount;
        ShowPlaceholder(I18NManager.GetResource(HomePageResources.CardPreviewLoadingTextBlockText));
        Loaded += HomePageCardControl_OnLoaded;
    }

    public HomePageCardControl()
    {
        ShowPlaceholder(I18NManager.GetResource(HomePageResources.CardPreviewLoadingTextBlockText));
    }

    public event EventHandler<HomeCardSelectedEventArgs>? CardSelected;

    public event EventHandler<HomeCardEditPreviewEventArgs>? EditPreview;

    public event EventHandler<HomeCardEditCompletedEventArgs>? EditCompleted;

    public HomePageCardLayout Card
    {
        get;
        private set => SetAndRaise(CardProperty, ref field, value);
    } = new();

    public HomeCardTemplate CardTemplate
    {
        get;
        private set => SetAndRaise(CardTemplateProperty, ref field, value);
    } = _PlaceholderTemplate;

    public int RowCount
    {
        get;
        private set => SetAndRaise(RowCountProperty, ref field, value);
    } = 1;

    public int ColumnCount
    {
        get;
        private set => SetAndRaise(ColumnCountProperty, ref field, value);
    } = 1;

    public string CardTitle
    {
        get;
        private set => SetAndRaise(CardTitleProperty, ref field, value);
    } = "";

    public Symbol CardSymbol
    {
        get;
        private set => SetAndRaise(CardSymbolProperty, ref field, value);
    }

    public bool IsEditing
    {
        get => GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public string? PlaceholderText
    {
        get;
        private set => SetAndRaise(PlaceholderTextProperty, ref field, value);
    }

    public object? PreviewContent
    {
        get;
        private set
        {
            if (ReferenceEquals(field, value))
                return;

            var isPreviewVisible = value is not null;
            SetAndRaise(PreviewContentProperty, ref field, value);
            IsPreviewVisible = isPreviewVisible;
            IsPlaceholderVisible = !isPreviewVisible;
        }
    }

    public bool IsPreviewVisible
    {
        get;
        private set => SetAndRaise(IsPreviewVisibleProperty, ref field, value);
    }

    public bool IsPlaceholderVisible
    {
        get;
        private set => SetAndRaise(IsPlaceholderVisibleProperty, ref field, value);
    }

    public void CancelEdit()
    {
        CompleteEdit();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachTemplateHandlers();

        base.OnApplyTemplate(e);

        _rootGrid = e.NameScope.Find<Grid>(PartRootGrid);
        _resizeHandlesLayer = e.NameScope.Find<Grid>(PartResizeHandlesLayer);

        if (_rootGrid is not null)
        {
            _rootGrid.PointerCaptureLost += Card_OnPointerCaptureLost;
            _rootGrid.PointerMoved += Card_OnPointerMoved;
            _rootGrid.PointerPressed += Card_OnPointerPressed;
            _rootGrid.PointerReleased += Card_OnPointerReleased;
        }

        if (_resizeHandlesLayer is not null)
            _resizeHandlesLayer.PointerPressed += ResizeHandle_OnPointerPressed;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CardTemplateProperty)
        {
            ApplyCardTemplate(change.GetNewValue<HomeCardTemplate>());
            return;
        }

        if (change.Property == IsEditingProperty)
        {
            PseudoClasses.Set(PcEditing, change.GetNewValue<bool>());

            if (change.GetOldValue<bool>() && !change.GetNewValue<bool>())
                CompleteEdit();

            return;
        }

        if (change.Property == IsSelectedProperty)
            PseudoClasses.Set(PcSelected, change.GetNewValue<bool>());
    }

    private void DetachTemplateHandlers()
    {
        _rootGrid?.PointerCaptureLost -= Card_OnPointerCaptureLost;
        _rootGrid?.PointerMoved -= Card_OnPointerMoved;
        _rootGrid?.PointerPressed -= Card_OnPointerPressed;
        _rootGrid?.PointerReleased -= Card_OnPointerReleased;

        _resizeHandlesLayer?.PointerPressed -= ResizeHandle_OnPointerPressed;

        _rootGrid = null;
        _resizeHandlesLayer = null;
    }
}
