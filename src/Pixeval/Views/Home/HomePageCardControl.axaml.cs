// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using Pixeval.Models.Home;
using Pixeval.Models.Options;
using Pixeval.ViewModels.Home;

namespace Pixeval.Views.Home;

public sealed partial class HomePageCardControl : TemplatedControl, IDisposable
{
    private const string PcEditing = ":editing";
    private const string PcSelected = ":selected";

    public static readonly DirectProperty<HomePageCardControl, HomePageCardLayout> CardProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, HomePageCardLayout>(nameof(Card), control => control.Card, (control, value) => control.Card = value);

    public static readonly DirectProperty<HomePageCardControl, HomeCardDefinition> CardDefinitionProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, HomeCardDefinition>(nameof(CardDefinition), control => control.CardDefinition,
            (control, value) => control.CardDefinition = value);

    public static readonly DirectProperty<HomePageCardControl, int> RowCountProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, int>(nameof(RowCount), control => control.RowCount, (control, value) => control.RowCount = value);

    public static readonly DirectProperty<HomePageCardControl, int> ColumnCountProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, int>(nameof(ColumnCount), control => control.ColumnCount, (control, value) => control.ColumnCount = value);

    public static readonly StyledProperty<bool> IsEditingProperty =
        AvaloniaProperty.Register<HomePageCardControl, bool>(nameof(IsEditing));

    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<HomePageCardControl, bool>(nameof(IsSelected));

    public static readonly StyledProperty<bool> IsCardTitleVisibleProperty =
        AvaloniaProperty.Register<HomePageCardControl, bool>(nameof(IsCardTitleVisible), true);

    public static readonly DirectProperty<HomePageCardControl, string> CardTitleProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, string>(nameof(CardTitle), control => control.CardTitle);

    public static readonly DirectProperty<HomePageCardControl, Symbol> CardSymbolProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, Symbol>(nameof(CardSymbol), control => control.CardSymbol);

    public static readonly DirectProperty<HomePageCardControl, HomeCardPreviewViewModel?> PreviewViewModelProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, HomeCardPreviewViewModel?>(nameof(PreviewViewModel), control => control.PreviewViewModel,
            (control, value) => control.PreviewViewModel = value);

    private static readonly HomeCardDefinition _PlaceholderDefinition = HomeCardDefinitions.Get(HomePageCardSourceKind.WorkRecommended);

    private Panel? _rootGrid;
    private Panel? _resizeHandlesLayer;
    private Button? _quickDeleteButton;
    private PointerEditState? _pointerEditState;
    private bool _isDisposed;

    public HomePageCardControl(
        HomePageCardLayout card,
        HomeCardDefinition definition,
        int rowCount,
        int columnCount)
    {
        Card = card;
        CardDefinition = definition;
        RowCount = rowCount;
        ColumnCount = columnCount;
        PreviewViewModel = new(card);
        Loaded += HomePageCardControl_OnLoaded;
    }

    public HomePageCardControl()
    {
        PreviewViewModel = new(Card);
    }

    public event EventHandler<HomeCardSelectedEventArgs>? CardSelected;

    public event EventHandler<HomeCardEditPreviewEventArgs>? EditPreview;

    public event EventHandler<HomeCardEditCompletedEventArgs>? EditCompleted;

    public event EventHandler<HomeCardDeleteRequestedEventArgs>? DeleteRequested;

    public HomePageCardLayout Card
    {
        get;
        private set
        {
            var oldTitle = CardTitle;
            SetAndRaise(CardProperty, ref field, value);
            RaisePropertyChanged(CardTitleProperty, oldTitle, CardTitle);
        }
    } = new();

    public HomeCardDefinition CardDefinition
    {
        get;
        private set
        {
            var oldTitle = CardTitle;
            var oldSymbol = CardSymbol;
            SetAndRaise(CardDefinitionProperty, ref field, value);
            RaisePropertyChanged(CardTitleProperty, oldTitle, CardTitle);
            RaisePropertyChanged(CardSymbolProperty, oldSymbol, CardSymbol);
            Background = new SolidColorBrush(Color.FromUInt32(Card.BackgroundColor));
        }
    } = _PlaceholderDefinition;

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

    public string CardTitle => Card.BuildTitle();

    public Symbol CardSymbol => CardDefinition.Symbol;

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

    public bool IsCardTitleVisible
    {
        get => GetValue(IsCardTitleVisibleProperty);
        set => SetValue(IsCardTitleVisibleProperty, value);
    }

    public HomeCardPreviewViewModel? PreviewViewModel
    {
        get;
        private set
        {
            if (ReferenceEquals(field, value))
                return;

            var old = field;
            SetAndRaise(PreviewViewModelProperty, ref field, value);
            old?.Dispose();
        }
    }

    public void CancelEdit() => CompleteEdit();

    public void UpdateGridSize(int rowCount, int columnCount)
    {
        RowCount = rowCount;
        ColumnCount = columnCount;
    }

    public void UpdateBackground()
    {
        Background = new SolidColorBrush(Color.FromUInt32(Card.BackgroundColor));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachTemplateHandlers();

        base.OnApplyTemplate(e);

        _rootGrid = e.NameScope.Find<Panel>(PartRootGrid);
        _resizeHandlesLayer = e.NameScope.Find<Panel>(PartResizeHandlesLayer);
        _quickDeleteButton = e.NameScope.Find<Button>(PartQuickDeleteButton);

        if (_rootGrid is not null)
        {
            _rootGrid.PointerCaptureLost += Card_OnPointerCaptureLost;
            _rootGrid.PointerMoved += Card_OnPointerMoved;
            _rootGrid.PointerPressed += Card_OnPointerPressed;
            _rootGrid.PointerReleased += Card_OnPointerReleased;
        }

        _resizeHandlesLayer?.PointerPressed += ResizeHandle_OnPointerPressed;
        _quickDeleteButton?.Click += QuickDeleteButton_OnClick;
    }

    private async void HomePageCardControl_OnLoaded(object? sender, RoutedEventArgs e)
    {
        Loaded -= HomePageCardControl_OnLoaded;
        if (PreviewViewModel is not null)
            await PreviewViewModel.LoadAsync();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

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
        _quickDeleteButton?.Click -= QuickDeleteButton_OnClick;

        _rootGrid = null;
        _resizeHandlesLayer = null;
        _quickDeleteButton = null;
    }

    private void QuickDeleteButton_OnClick(object? sender, RoutedEventArgs e)
    {
        DeleteRequested?.Invoke(this, new(Card));
        e.Handled = true;
    }

    [RelayCommand]
    private void OpenPreviewItem(object? parameter)
    {
        if (TopLevel.GetTopLevel(this) is not { } topLevel)
            return;

        HomeCardDefinitions.OpenPreviewItem(PreviewViewModel, topLevel, parameter);
    }

    [RelayCommand]
    private async Task OpenCardPageAsync()
    {
        if (PreviewViewModel is null || TopLevel.GetTopLevel(this) is not { } topLevel)
            return;

        await PreviewViewModel.EnsureLoadedAsync();
        CardDefinition.OpenCardPage(Card, PreviewViewModel, topLevel);
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        Loaded -= HomePageCardControl_OnLoaded;
        CompleteEdit(false);
        DetachTemplateHandlers();
        CardSelected = null;
        EditPreview = null;
        EditCompleted = null;
        DeleteRequested = null;
        PreviewViewModel = null;
    }
}
