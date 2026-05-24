// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Pixeval.Models.Home;
using Pixeval.Views.Home;

namespace Pixeval.Views;

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

    private void RefreshSelectionVisuals()
    {
        foreach (var child in HomeGrid.Children)
            if (child is HomePageCardControl control)
                control.IsSelected = control.Card == _selectedCard;
    }

    private void RefreshEditModeVisuals()
    {
        var isEditing = EditModeButton.IsChecked is true;
        foreach (var child in HomeGrid.Children)
            if (child is HomePageCardControl control)
                control.IsEditing = isEditing;
    }

    private void AddCardControl(HomePageCardLayout card)
    {
        var control = new HomePageCardControl(card, GetTemplate(card), RowCount, ColumnCount)
        {
            IsEditing = EditModeButton.IsChecked is true,
            IsSelected = card == _selectedCard
        };
        control.CardSelected += HomeCardControl_OnCardSelected;
        control.EditPreview += HomeCardControl_OnEditPreview;
        control.EditCompleted += HomeCardControl_OnEditCompleted;
        Grid.SetColumn(control, card.Column);
        Grid.SetRow(control, card.Row);
        Grid.SetColumnSpan(control, card.ColumnSpan);
        Grid.SetRowSpan(control, card.RowSpan);
        HomeGrid.Children.Add(control);
    }

    private void RemoveCardControl(HomePageCardLayout card)
    {
        for (var i = HomeGrid.Children.Count - 1; i >= 0; i--)
            if (HomeGrid.Children[i] is HomePageCardControl control && control.Card == card)
            {
                control.CancelEdit();
                HomeGrid.Children.RemoveAt(i);
                return;
            }
    }

    private void UpdateSelectedCardLayoutVisual()
    {
        if (_selectedCard is not { } card)
            return;

        foreach (var child in HomeGrid.Children)
            if (child is HomePageCardControl control && control.Card == card)
            {
                Grid.SetColumn(control, card.Column);
                Grid.SetRow(control, card.Row);
                Grid.SetColumnSpan(control, card.ColumnSpan);
                Grid.SetRowSpan(control, card.RowSpan);
                return;
            }
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

    private void CreateDefaultCards()
    {
        _cards.Add(CreateCard(_cardTemplates[0], 0, 0, Math.Min(3, ColumnCount), Math.Min(2, RowCount)));
        if (ColumnCount >= 5)
            _cards.Add(CreateCard(_cardTemplates[1], 3, 0, Math.Min(2, ColumnCount - 3), Math.Min(2, RowCount)));
        if (RowCount >= 4)
            _cards.Add(CreateCard(_cardTemplates[3], 0, 2, Math.Min(3, ColumnCount), Math.Min(2, RowCount - 2)));
        if (ColumnCount >= 5 && RowCount >= 5)
            _cards.Add(CreateCard(_cardTemplates[4], 3, 2, Math.Min(2, ColumnCount - 3), Math.Min(3, RowCount - 2)));
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
        var maxColumnSpan = Math.Min(preferredColumnSpan, ColumnCount);
        var maxRowSpan = Math.Min(preferredRowSpan, RowCount);
        var candidates = Enumerable.Range(1, maxRowSpan)
            .SelectMany(height => Enumerable.Range(1, maxColumnSpan), (height, width) => new
            {
                height,
                width
            })
            .OrderByDescending(t => t.width * t.height)
            .ThenBy(t => Math.Abs(preferredColumnSpan - t.width) + Math.Abs(preferredRowSpan - t.height))
            .ThenByDescending(t => t.height)
            .ThenByDescending(t => t.width)
            .Select(t => (Width: t.width, Height: t.height));

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
        card.ColumnSpan = Math.Clamp(card.ColumnSpan, 1, ColumnCount);
        card.RowSpan = Math.Clamp(card.RowSpan, 1, RowCount);
        card.Column = Math.Clamp(card.Column, 0, ColumnCount - card.ColumnSpan);
        card.Row = Math.Clamp(card.Row, 0, RowCount - card.RowSpan);
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
