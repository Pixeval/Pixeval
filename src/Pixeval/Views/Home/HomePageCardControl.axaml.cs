// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using Pixeval.Models.Home;
using Pixeval.Utilities;
using Pixeval.ViewModels.Home;

namespace Pixeval.Views.Home;

public sealed partial class HomePageCardControl : UserControl, IDisposable
{
    public static readonly StyledProperty<bool> IsEditingProperty =
        AvaloniaProperty.Register<HomePageCardControl, bool>(nameof(IsEditing));

    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<HomePageCardControl, bool>(nameof(IsSelected));

    private readonly bool _loadPreview;
    private PointerEditState? _pointerEditState;
    private bool _isDisposed;

    public HomePageCardControl(
        HomePageCardLayout card,
        int rowCount,
        int columnCount) : this(card, rowCount, columnCount, true)
    {
    }

    private HomePageCardControl(
        HomePageCardLayout card,
        int rowCount,
        int columnCount,
        bool loadPreview)
    {
        Card = card;
        Definition = HomeCardDefinitions.Get(card.SourceKind);
        RowCount = rowCount;
        ColumnCount = columnCount;
        PreviewViewModel = new(card, Definition.CreatePreviewSourceAsync);
        _loadPreview = loadPreview;
        InitializeComponent();
        UpdateBackground();
    }

    public HomePageCardControl() : this(new(), 1, 1, false)
    {
    }

    public event EventHandler<HomeCardSelectedEventArgs>? CardSelected;

    public event EventHandler<HomeCardEditPreviewEventArgs>? EditPreview;

    public event EventHandler<HomeCardEditCompletedEventArgs>? EditCompleted;

    public event EventHandler<HomeCardDeleteRequestedEventArgs>? DeleteRequested;

    public HomePageCardLayout Card { get; }

    private HomeCardDefinition Definition { get; }

    public int RowCount { get; private set; }

    public int ColumnCount { get; private set; }

    public string CardTitle => Definition.BuildTitle(Card);

    public Symbol CardSymbol => Definition.Symbol;

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

    public HomeCardPreviewViewModel PreviewViewModel { get; }

    public void CancelEdit() => CompleteEdit();

    public void UpdateGridSize(int rowCount, int columnCount)
    {
        RowCount = rowCount;
        ColumnCount = columnCount;
    }

    public void UpdateBackground()
    {
        CardBorder.Background = new SolidColorBrush(Color.FromUInt32(Card.BackgroundColor));
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (_loadPreview)
            await PreviewViewModel.LoadAsync();
    }

    private void QuickDeleteButton_OnClick(object? sender, RoutedEventArgs e)
    {
        DeleteRequested?.Invoke(this, new(Card));
        e.Handled = true;
    }

    [RelayCommand]
    private void OpenPreviewItem(object? parameter)
    {
        if (parameter is not Control control
            || TopLevel.GetTopLevel(control) is not { } topLevel)
            return;

        HomeCardDefinitions.OpenPreviewItem(topLevel, control.DataContext, PreviewViewModel.ViewModel);
    }

    [RelayCommand]
    private async Task OpenCardPageAsync()
    {
        if (TopLevel.GetTopLevel(this) is not { } topLevel
            || topLevel.ViewContainer is null)
            return;

        await PreviewViewModel.EnsureLoadedAsync();
        var source = PreviewViewModel.CloneSource();
        if (source is null)
            return;

        Definition.OpenCardPage(Card, source, topLevel);
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        CompleteEdit(false);
        CardSelected = null;
        EditPreview = null;
        EditCompleted = null;
        DeleteRequested = null;
        PreviewViewModel.Dispose();
    }
}
