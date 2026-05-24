// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Pixeval.AppManagement;
using Pixeval.I18N;
using Pixeval.Models.Home;
using Pixeval.Utilities;
using Pixeval.Views.Home;

namespace Pixeval.Views;

public partial class HomePage
{
    private void EditModeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var isEditing = EditModeButton.IsChecked is true;
        EditPane.IsVisible = isEditing;
        if (!isEditing)
        {
            _activeCardControl?.CancelEdit();
            _activeCardControl = null;
            SelectCard(null);
        }
        else
        {
            UpdateGridSizeControls();
        }
        RefreshGrid();
    }

    private void ResetLayoutButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _cards.Clear();
        SelectCard(null);
        CreateDefaultCards();
        SaveLayout();
        RefreshGrid();
    }

    private void AddCardButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: HomeCardTemplate template })
            return;

        var width = Math.Min(template.DefaultColumnSpan, ColumnCount);
        var height = Math.Min(template.DefaultRowSpan, RowCount);
        if (!TryFindFreePosition(width, height, out var column, out var row)
            && !TryFindBestFittingFreePosition(width, height, out width, out height, out column, out row))
        {
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowWarning(
                I18NManager.GetResource(HomePageResources.NoSpaceWarningTitle),
                I18NManager.GetResource(HomePageResources.NoSpaceWarningContent));
            return;
        }

        var card = CreateCard(template, column, row, width, height);
        _cards.Add(card);
        SelectCard(card);
        SaveLayout();
        RefreshGrid();
    }

    private void DeleteSelectedCardButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_selectedCard is null)
            return;

        _ = _cards.Remove(_selectedCard);
        SelectCard(null);
        SaveLayout();
        RefreshGrid();
    }

    private void GridSizeBox_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (_isUpdatingGridSizeControls || sender is not NumericUpDown)
            return;

        var rows = Math.Clamp(DecimalToPositiveInt(GridRowsBox.Value), MinimumGridSize, MaximumGridSize);
        var columns = Math.Clamp(DecimalToPositiveInt(GridColumnsBox.Value), MinimumGridSize, MaximumGridSize);
        var settings = App.AppViewModel.AppSettings;
        if (settings.HomePageRows == rows && settings.HomePageColumns == columns)
            return;

        settings.HomePageRows = rows;
        settings.HomePageColumns = columns;
        SaveLayout();
        RefreshGrid();
        UpdateSelectedCardControls();
        UpdateGridSizeControls();
    }

    private void SelectedCardNumericBox_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (_isUpdatingSelectedCardControls || _selectedCard is null || sender is not NumericUpDown)
            return;

        var newColumn = DecimalToZeroBasedInt(SelectedColumnBox.Value);
        var newRow = DecimalToZeroBasedInt(SelectedRowBox.Value);
        var newColumnSpan = DecimalToPositiveInt(SelectedWidthBox.Value);
        var newRowSpan = DecimalToPositiveInt(SelectedHeightBox.Value);

        if (newColumn + newColumnSpan > ColumnCount)
            newColumnSpan = ColumnCount - newColumn;
        if (newRow + newRowSpan > RowCount)
            newRowSpan = RowCount - newRow;

        if (newColumnSpan < 1 || newRowSpan < 1
            || !CanPlace(_selectedCard, newColumn, newRow, newColumnSpan, newRowSpan))
        {
            UpdateSelectedCardControls();
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowWarning(
                I18NManager.GetResource(HomePageResources.LayoutConflictWarningTitle),
                I18NManager.GetResource(HomePageResources.LayoutConflictWarningContent));
            return;
        }

        _selectedCard.Column = newColumn;
        _selectedCard.Row = newRow;
        _selectedCard.ColumnSpan = newColumnSpan;
        _selectedCard.RowSpan = newRowSpan;
        SaveLayout();
        RefreshGrid();
        UpdateSelectedCardControls();
    }

    private void HomeCardControl_OnCardSelected(object? sender, HomeCardSelectedEventArgs e)
    {
        SelectCard(e.Card);
        if (sender is HomePageCardControl cardControl)
            _activeCardControl = cardControl;

        if (e.RefreshSelection)
            RefreshGrid();
    }

    private void HomeCardControl_OnEditPreview(object? sender, HomeCardEditPreviewEventArgs e)
    {
        e.Accepted = CanPlace(e.Card, e.Bounds);
    }

    private void HomeCardControl_OnEditCompleted(object? sender, HomeCardEditCompletedEventArgs e)
    {
        if (sender is HomePageCardControl cardControl && ReferenceEquals(_activeCardControl, cardControl))
            _activeCardControl = null;

        if (e.HasChanged)
            SaveLayout();

        RefreshGrid();
        UpdateSelectedCardControls();
    }

    private void SelectCard(HomePageCardLayout? card)
    {
        _selectedCard = card;
        DeleteSelectedCardButton.IsEnabled = card is not null;
        SelectedCardTextBlock.Text = card is null
            ? I18NManager.GetResource(HomePageResources.NoSelectedCardTextBlockText)
            : GetTemplate(card.Kind).Title;
        UpdateSelectedCardControls();
    }

    private void UpdateSelectedCardControls()
    {
        _isUpdatingSelectedCardControls = true;
        try
        {
            SelectedColumnBox.Maximum = ColumnCount;
            SelectedRowBox.Maximum = RowCount;
            SelectedWidthBox.Maximum = ColumnCount;
            SelectedHeightBox.Maximum = RowCount;

            var card = _selectedCard;
            SelectedColumnBox.IsEnabled = card is not null;
            SelectedRowBox.IsEnabled = card is not null;
            SelectedWidthBox.IsEnabled = card is not null;
            SelectedHeightBox.IsEnabled = card is not null;

            SelectedColumnBox.Value = card is null ? 1 : card.Column + 1;
            SelectedRowBox.Value = card is null ? 1 : card.Row + 1;
            SelectedWidthBox.Value = card?.ColumnSpan ?? 1;
            SelectedHeightBox.Value = card?.RowSpan ?? 1;
        }
        finally
        {
            _isUpdatingSelectedCardControls = false;
        }
    }

    private void UpdateGridSizeControls()
    {
        _isUpdatingGridSizeControls = true;
        try
        {
            GridRowsBox.Maximum = MaximumGridSize;
            GridColumnsBox.Maximum = MaximumGridSize;
            GridRowsBox.Minimum = MinimumGridSize;
            GridColumnsBox.Minimum = MinimumGridSize;
            GridRowsBox.Value = RowCount;
            GridColumnsBox.Value = ColumnCount;
        }
        finally
        {
            _isUpdatingGridSizeControls = false;
        }
    }

    private HomeCardTemplate GetTemplate(HomePageCardKind kind) =>
        _cardTemplates.FirstOrDefault(template => template.Kind == kind) ?? _cardTemplates[0];

    private static int DecimalToZeroBasedInt(decimal? value) => Math.Max(0, DecimalToPositiveInt(value) - 1);

    private static int DecimalToPositiveInt(decimal? value) => Math.Max(1, (int) (value ?? 1));

    private static HomePageCardLayout CreateCard(HomeCardTemplate template, int column, int row, int columnSpan, int rowSpan) => new()
    {
        Kind = template.Kind,
        Column = column,
        Row = row,
        ColumnSpan = columnSpan,
        RowSpan = rowSpan
    };

    private static void SaveLayout() => AppInfo.SaveSettings(App.AppViewModel.AppSettings);
}
