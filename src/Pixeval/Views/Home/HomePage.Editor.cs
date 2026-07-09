// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Pixeval.AppManagement;
using Pixeval.I18N;
using Pixeval.Models.Home;
using Pixeval.Utilities;

namespace Pixeval.Views.Home;

public sealed partial class HomePage
{
    [RelayCommand]
    private void AddConfiguredCard()
    {
        if (ViewModel.IsAddingConfiguredCard || ViewModel.PendingTemplate is not { } template)
            return;

        ViewModel.SetAddingConfiguredCard(true);
        try
        {
            if (!ViewModel.TryCreateCardFromSourceParameters(out var draft) || draft is null)
            {
                ShowInvalidParameterWarning();
                return;
            }

            var width = int.Min(template.DefaultColumnSpan, ColumnCount);
            var height = int.Min(template.DefaultRowSpan, RowCount);
            if (!TryFindFreePosition(width, height, out var column, out var row)
                && !TryFindBestFittingFreePosition(width, height, out width, out height, out column, out row))
            {
                TopLevel.GetTopLevel(this)?.ViewContainer?.ShowWarning(
                    I18NManager.GetResource(HomePageResources.NoSpaceWarningTitle),
                    I18NManager.GetResource(HomePageResources.NoSpaceWarningContent));
                return;
            }

            draft.Column = column;
            draft.Row = row;
            draft.ColumnSpan = width;
            draft.RowSpan = height;
            _cards.Add(draft);
            SelectCard(draft);
            SaveLayout();
            AddCardControl(draft);
            RefreshSelectionVisuals();
        }
        finally
        {
            ViewModel.SetAddingConfiguredCard(false);
        }
    }

    [RelayCommand]
    private void DeleteSelectedCard()
    {
        if (_selectedCard is { } card)
            DeleteCard(card);
    }

    private void ApplyGridSizeFromViewModel()
    {
        var rows = DecimalToPositiveInt(decimal.Clamp(ViewModel.GridRowsValue, MinimumGridSize, MaximumGridSize));
        var columns = DecimalToPositiveInt(decimal.Clamp(ViewModel.GridColumnsValue, MinimumGridSize, MaximumGridSize));
        var settings = App.AppViewModel.AppSettings.ApplicationSettings;
        if (settings.HomePageRows == rows && settings.HomePageColumns == columns)
            return;

        if (!CanResizeGrid(rows, columns))
        {
            UpdateGridSizeControls();
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowWarning(
                I18NManager.GetResource(HomePageResources.GridShrinkBlockedWarningTitle),
                I18NManager.GetResource(HomePageResources.GridShrinkBlockedWarningContent));
            return;
        }

        settings.HomePageRows = rows;
        settings.HomePageColumns = columns;
        SaveLayout();
        ViewModel.NotifyGridSizeChanged();
        RefreshGridSize();
        UpdateSelectedCardControls();
        UpdateGridSizeControls();
    }

    private void ApplySelectedCardLayoutFromViewModel()
    {
        if (_selectedCard is null)
            return;

        var newColumn = DecimalToZeroBasedInt(ViewModel.SelectedColumnValue);
        var newRow = DecimalToZeroBasedInt(ViewModel.SelectedRowValue);
        var newColumnSpan = DecimalToPositiveInt(ViewModel.SelectedWidthValue);
        var newRowSpan = DecimalToPositiveInt(ViewModel.SelectedHeightValue);

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
        UpdateSelectedCardLayoutVisual();
        UpdateSelectedCardControls();
    }

    private void ApplySelectedCardBackgroundFromViewModel()
    {
        if (_selectedCard is null)
            return;

        var color = ViewModel.SelectedBackgroundColor.ToUInt32();
        if (_selectedCard.BackgroundColor == color)
            return;

        _selectedCard.BackgroundColor = color;
        SaveLayout();
        UpdateSelectedCardLayoutVisual();
    }

    private void HomeCardControl_OnCardSelected(object? sender, HomeCardSelectedEventArgs e)
    {
        SelectCard(e.Card);
        if (sender is HomePageCardControl cardControl)
            _activeCardControl = cardControl;

        if (e.RefreshSelection)
            RefreshSelectionVisuals();
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

        RefreshSelectionVisuals();
        UpdateSelectedCardControls();
    }

    private void HomeCardControl_OnDeleteRequested(object? sender, HomeCardDeleteRequestedEventArgs e)
    {
        DeleteCard(e.Card);
    }

    private void DeleteCard(HomePageCardLayout card)
    {
        if (!_cards.Remove(card))
            return;

        if (_activeCardControl?.Card == card)
            _activeCardControl = null;

        RemoveCardControl(card);
        if (_selectedCard == card)
            SelectCard(null);

        SaveLayout();
    }

    private void SelectCard(HomePageCardLayout? card)
    {
        _selectedCard = card;
        _isUpdatingSelectedCardControls = true;
        try
        {
            ViewModel.SetSelectedCard(card);
        }
        finally
        {
            _isUpdatingSelectedCardControls = false;
        }

        if (!_isRefreshingGrid)
            RefreshSelectionVisuals();
    }

    private void UpdateSelectedCardControls()
    {
        _isUpdatingSelectedCardControls = true;
        try
        {
            ViewModel.SyncSelectedCardEditorValues();
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
            ViewModel.SyncGridEditorValues();
        }
        finally
        {
            _isUpdatingGridSizeControls = false;
        }
    }

    private HomePageCardLayout? ShowInvalidParameterWarning()
    {
        TopLevel.GetTopLevel(this)?.ViewContainer?.ShowWarning(
            I18NManager.GetResource(HomePageResources.InvalidSourceParameterWarningTitle),
            I18NManager.GetResource(HomePageResources.InvalidSourceParameterWarningContent));
        return null;
    }

    private static int DecimalToZeroBasedInt(decimal? value) => int.Max(0, DecimalToPositiveInt(value) - 1);

    private static int DecimalToPositiveInt(decimal? value) => int.Max(1, (int) (value ?? 1));

    private static void SaveLayout()
    {
        AppInfo.SaveAppSettings(App.AppViewModel.AppSettings);
        AppInfo.SaveHomePageCards(App.AppViewModel.HomePageCards);
    }
}
