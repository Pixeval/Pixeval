// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Pixeval.Models.Home;

namespace Pixeval.Views.Home;

public partial class HomePage
{
    private void RefreshGrid()
    {
        _isRefreshingGrid = true;
        try
        {
            NormalizeCards();
            EnsureGridDefinitions(HomeGrid);
            EnsureGuideGrid();
            foreach (var child in HomeGrid.Children.OfType<HomePageCardControl>())
                DisposeCardControl(child);

            HomeGrid.Children.Clear();

            foreach (var card in _cards)
            {
                AddCardControl(card);
            }
        }
        finally
        {
            _isRefreshingGrid = false;
        }
    }

    private void RefreshGridSize()
    {
        _isRefreshingGrid = true;
        try
        {
            _activeCardControl?.CancelEdit();
            _activeCardControl = null;
            NormalizeCards();
            EnsureGridDefinitions(HomeGrid);
            EnsureGuideGrid();
            SyncCardControls();
        }
        finally
        {
            _isRefreshingGrid = false;
        }

        RefreshSelectionVisuals();
    }

    private void RefreshSelectionVisuals()
    {
        foreach (var child in HomeGrid.Children)
            if (child is HomePageCardControl control)
                control.IsSelected = control.Card == _selectedCard;
    }

    private void RefreshEditModeVisuals()
    {
        var isEditing = ViewModel.IsEditMode;
        foreach (var child in HomeGrid.Children)
            if (child is HomePageCardControl control)
                control.IsEditing = isEditing;
    }

    private void AddCardControl(HomePageCardLayout card)
    {
        var control = new HomePageCardControl(card, ViewModel.GetTemplate(card), RowCount, ColumnCount)
        {
            IsCardTitleVisible = !ViewModel.IsCardTitleHidden,
            IsEditing = ViewModel.IsEditMode,
            IsSelected = card == _selectedCard
        };
        control.CardSelected += HomeCardControl_OnCardSelected;
        control.EditPreview += HomeCardControl_OnEditPreview;
        control.EditCompleted += HomeCardControl_OnEditCompleted;
        control.DeleteRequested += HomeCardControl_OnDeleteRequested;
        Grid.SetColumn(control, card.Column);
        Grid.SetRow(control, card.Row);
        Grid.SetColumnSpan(control, card.ColumnSpan);
        Grid.SetRowSpan(control, card.RowSpan);
        HomeGrid.Children.Add(control);
    }

    private void SyncCardControls()
    {
        for (var i = HomeGrid.Children.Count - 1; i >= 0; i--)
            if (HomeGrid.Children[i] is HomePageCardControl control && !_cards.Any(card => ReferenceEquals(card, control.Card)))
            {
                DisposeCardControl(control);
                HomeGrid.Children.RemoveAt(i);
            }

        foreach (var card in _cards)
        {
            if (TryGetCardControl(card, out var control))
            {
                control.UpdateGridSize(RowCount, ColumnCount);
                control.UpdateBackground();
                control.IsCardTitleVisible = !ViewModel.IsCardTitleHidden;
                control.IsEditing = ViewModel.IsEditMode;
                control.IsSelected = card == _selectedCard;
                ApplyCardLayout(control);
            }
            else
            {
                AddCardControl(card);
            }
        }
    }

    private void RemoveCardControl(HomePageCardLayout card)
    {
        for (var i = HomeGrid.Children.Count - 1; i >= 0; i--)
            if (HomeGrid.Children[i] is HomePageCardControl control && control.Card == card)
            {
                control.CancelEdit();
                DisposeCardControl(control);
                HomeGrid.Children.RemoveAt(i);
                return;
            }
    }

    private void UpdateSelectedCardLayoutVisual()
    {
        if (_selectedCard is not { } card)
            return;

        if (TryGetCardControl(card, out var control))
        {
            control.UpdateBackground();
            ApplyCardLayout(control);
        }
    }

    private bool TryGetCardControl(HomePageCardLayout card, out HomePageCardControl control)
    {
        foreach (var child in HomeGrid.Children)
            if (child is HomePageCardControl candidate && ReferenceEquals(candidate.Card, card))
            {
                control = candidate;
                return true;
            }

        control = null!;
        return false;
    }

    private void DetachCardControl(HomePageCardControl control)
    {
        control.CardSelected -= HomeCardControl_OnCardSelected;
        control.EditPreview -= HomeCardControl_OnEditPreview;
        control.EditCompleted -= HomeCardControl_OnEditCompleted;
        control.DeleteRequested -= HomeCardControl_OnDeleteRequested;
    }

    private void DisposeCardControl(HomePageCardControl control)
    {
        DetachCardControl(control);
        control.Dispose();
    }

    private static void ApplyCardLayout(HomePageCardControl control)
    {
        var card = control.Card;
        Grid.SetColumn(control, card.Column);
        Grid.SetRow(control, card.Row);
        Grid.SetColumnSpan(control, card.ColumnSpan);
        Grid.SetRowSpan(control, card.RowSpan);
    }

    private void EnsureGridDefinitions(Grid grid)
    {
        grid.RowDefinitions.Clear();
        grid.ColumnDefinitions.Clear();

        for (var row = 0; row < RowCount; row++)
            grid.RowDefinitions.Add(new(GridLength.Star));
        for (var column = 0; column < ColumnCount; column++)
            grid.ColumnDefinitions.Add(new(GridLength.Star));
    }

    private void EnsureGuideGrid()
    {
        EnsureGridDefinitions(GuideGrid);
        GuideGrid.Children.Clear();

        for (var row = 0; row < RowCount; row++)
            for (var column = 0; column < ColumnCount; column++)
            {
                var guide = new HomeGridGuideCell();
                Grid.SetColumn(guide, column);
                Grid.SetRow(guide, row);
                GuideGrid.Children.Add(guide);
            }
    }

    private bool TryFindFreePosition(int columnSpan, int rowSpan, out int column, out int row)
    {
        return TryFindFreePosition(columnSpan, rowSpan, _cards, out column, out row);
    }

    private bool TryFindBestFittingFreePosition(
        int preferredColumnSpan,
        int preferredRowSpan,
        out int columnSpan,
        out int rowSpan,
        out int column,
        out int row)
    {
        var maxColumnSpan = int.Min(preferredColumnSpan, ColumnCount);
        var maxRowSpan = int.Min(preferredRowSpan, RowCount);
        var candidates = Enumerable.Range(1, maxRowSpan)
            .SelectMany(height => Enumerable.Range(1, maxColumnSpan), (height, width) => new
            {
                Width = width,
                Height = height
            })
            .OrderByDescending(t => t.Width * t.Height)
            .ThenBy(t => int.Abs(preferredColumnSpan - t.Width) + int.Abs(preferredRowSpan - t.Height))
            .ThenByDescending(t => t.Height)
            .ThenByDescending(t => t.Width);

        foreach (var candidate in candidates)
        {
            if (!TryFindFreePosition(candidate.Width, candidate.Height, out column, out row))
                continue;

            columnSpan = candidate.Width;
            rowSpan = candidate.Height;
            return true;
        }

        columnSpan = rowSpan = 1;
        column = row = 0;
        return false;
    }

    private bool TryFindFreePosition(int columnSpan, int rowSpan, IReadOnlyCollection<HomePageCardLayout> occupiedCards, out int column, out int row)
    {
        for (row = 0; row <= RowCount - rowSpan; row++)
            for (column = 0; column <= ColumnCount - columnSpan; column++)
                if (CanPlace(occupiedCards, column, row, columnSpan, rowSpan))
                    return true;

        column = row = 0;
        return false;
    }

    private bool CanPlace(HomePageCardLayout? movingCard, int column, int row, int columnSpan, int rowSpan)
    {
        if (column < 0 || row < 0 || columnSpan < 1 || rowSpan < 1
            || column + columnSpan > ColumnCount || row + rowSpan > RowCount)
        {
            return false;
        }

        return _cards.All(card => card == movingCard || !Overlaps(card.Column, card.Row, card.ColumnSpan, card.RowSpan, column, row, columnSpan, rowSpan));
    }

    private bool CanPlace(HomePageCardLayout movingCard, HomeCardBounds bounds) =>
        CanPlace(movingCard, bounds.Column, bounds.Row, bounds.ColumnSpan, bounds.RowSpan);

    private bool CanResizeGrid(int rowCount, int columnCount) =>
        _cards.All(card => card.Row + card.RowSpan <= rowCount && card.Column + card.ColumnSpan <= columnCount);

    private bool CanPlace(IReadOnlyCollection<HomePageCardLayout> occupiedCards, int column, int row, int columnSpan, int rowSpan)
    {
        if (column < 0 || row < 0 || columnSpan < 1 || rowSpan < 1
            || column + columnSpan > ColumnCount || row + rowSpan > RowCount)
        {
            return false;
        }

        return occupiedCards.All(card => !Overlaps(card.Column, card.Row, card.ColumnSpan, card.RowSpan, column, row, columnSpan, rowSpan));
    }

    private static bool Overlaps(int column1, int row1, int columnSpan1, int rowSpan1, int column2, int row2, int columnSpan2, int rowSpan2)
    {
        return column1 < column2 + columnSpan2
            && column1 + columnSpan1 > column2
            && row1 < row2 + rowSpan2
            && row1 + rowSpan1 > row2;
    }

    private bool ClampCard(HomePageCardLayout card)
    {
        var old = card with { };
        card.ColumnSpan = int.Clamp(card.ColumnSpan, 1, ColumnCount);
        card.RowSpan = int.Clamp(card.RowSpan, 1, RowCount);
        card.Column = int.Clamp(card.Column, 0, ColumnCount - card.ColumnSpan);
        card.Row = int.Clamp(card.Row, 0, RowCount - card.RowSpan);
        return old != card;
    }

    private void NormalizeCards()
    {
        var changed = false;
        var occupiedCards = new List<HomePageCardLayout>();

        foreach (var card in _cards.ToList())
        {
            changed |= ClampCard(card);

            if (CanPlace(occupiedCards, card.Column, card.Row, card.ColumnSpan, card.RowSpan))
            {
                occupiedCards.Add(card);
                continue;
            }

            if (TryFindFreePosition(card.ColumnSpan, card.RowSpan, occupiedCards, out var column, out var row)
                || TryShrinkAndFindFreePosition(card, occupiedCards, out column, out row))
            {
                card.Column = column;
                card.Row = row;
                occupiedCards.Add(card);
            }
            else
            {
                _ = _cards.Remove(card);
                if (card == _selectedCard)
                    SelectCard(null);
            }

            changed = true;
        }

        if (changed)
            SaveLayout();
    }

    private bool TryShrinkAndFindFreePosition(
        HomePageCardLayout card,
        IReadOnlyCollection<HomePageCardLayout> occupiedCards,
        out int column,
        out int row)
    {
        if (!TryFindFreePosition(1, 1, occupiedCards, out column, out row))
            return false;

        card.ColumnSpan = 1;
        card.RowSpan = 1;
        return true;
    }
}
