// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Mako;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Models.Home;
using Pixeval.Models.Options;
using Pixeval.Utilities;

namespace Pixeval.Views.Home;

public partial class HomePage
{
    private void EditModeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        SetEditMode(EditModeButton.IsChecked is true);
    }

    private void HideToolbarButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var settings = App.AppViewModel.AppSettings;
        settings.HideHomePageToolbar = HideToolbarButton.IsChecked is true;
        ApplyDisplaySettings();
        AppInfo.SaveSettings(settings);
    }

    private void HideCardTitleButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var settings = App.AppViewModel.AppSettings;
        settings.HideHomePageCardTitle = HideCardTitleButton.IsChecked is true;
        ApplyDisplaySettings();
        AppInfo.SaveSettings(settings);
    }

    private void InitializeDisplaySettingsControls()
    {
        var settings = App.AppViewModel.AppSettings;
        HideToolbarButton.IsChecked = settings.HideHomePageToolbar;
        HideCardTitleButton.IsChecked = settings.HideHomePageCardTitle;
    }

    private void ApplyDisplaySettings()
    {
        var settings = App.AppViewModel.AppSettings;
        if (settings.HideHomePageToolbar)
            SetEditMode(false);

        HomeToolbar.IsVisible = !settings.HideHomePageToolbar;
        HomeGridBorder.Margin = settings.HideHomePageToolbar ? new Thickness(0) : new Thickness(0, 12, 0, 0);

        foreach (var child in HomeGrid.Children.OfType<HomePageCardControl>())
            child.IsCardTitleVisible = !settings.HideHomePageCardTitle;
    }

    private void SetEditMode(bool isEditing)
    {
        if (EditModeButton.IsChecked != isEditing)
            EditModeButton.IsChecked = isEditing;

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
        RefreshEditModeVisuals();
    }

    private void CardTemplateButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: HomeCardTemplate template })
            return;

        _pendingTemplate = template;
        UpdateSourceParameterControls();
    }

    private void AddConfiguredCardButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_isAddingConfiguredCard || _pendingTemplate is not { } template)
            return;

        _isAddingConfiguredCard = true;
        AddConfiguredCardButton.IsEnabled = false;
        try
        {
            if (TryCreateCardFromSourceParameters(template) is not { } draft)
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
            _isAddingConfiguredCard = false;
            AddConfiguredCardButton.IsEnabled = _pendingTemplate is not null;
        }
    }

    private void SourceSimpleWorkTypeComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e)
    {
        UpdateRankOptionItems();
    }

    private void DeleteSelectedCardButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_selectedCard is { } card)
            DeleteCard(card);
    }

    private void GridSizeBox_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (_isUpdatingGridSizeControls || sender is not NumericUpDown)
            return;

        var rows = DecimalToPositiveInt(Math.Clamp(GridRowsBox.Value ?? 1, MinimumGridSize, MaximumGridSize));
        var columns = DecimalToPositiveInt(Math.Clamp(GridColumnsBox.Value ?? 1, MinimumGridSize, MaximumGridSize));
        var settings = App.AppViewModel.AppSettings;
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
        RefreshGridSize();
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
        UpdateSelectedCardLayoutVisual();
        UpdateSelectedCardControls();
    }

    private void SelectedBackgroundColorPicker_OnColorChanged(object? sender, ColorChangedEventArgs e)
    {
        if (_isUpdatingSelectedCardControls || _selectedCard is null)
            return;

        var color = e.NewColor.ToUInt32();
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
        DeleteSelectedCardButton.IsEnabled = card is not null;
        SelectedCardTextBlock.Text = card is null
            ? I18NManager.GetResource(HomePageResources.NoSelectedCardTextBlockText)
            : card.ToString();
        UpdateSelectedCardControls();
        if (!_isRefreshingGrid)
            RefreshSelectionVisuals();
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
            var cardIsNotNull = card is not null;
            SelectedColumnBox.IsEnabled = cardIsNotNull;
            SelectedRowBox.IsEnabled = cardIsNotNull;
            SelectedWidthBox.IsEnabled = cardIsNotNull;
            SelectedHeightBox.IsEnabled = cardIsNotNull;
            SelectedBackgroundColorPicker.IsEnabled = cardIsNotNull;

            SelectedColumnBox.Value = (card?.Column ?? 0) + 1;
            SelectedRowBox.Value = (card?.Row ?? 0) + 1;
            SelectedWidthBox.Value = card?.ColumnSpan ?? 1;
            SelectedHeightBox.Value = card?.RowSpan ?? 1;
            SelectedBackgroundColorPicker.Color = Color.FromUInt32(card?.BackgroundColor ?? 0);
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
            GridRowsBox.Value = RowCount;
            GridColumnsBox.Value = ColumnCount;
        }
        finally
        {
            _isUpdatingGridSizeControls = false;
        }
    }

    private void InitializeSourceParameterControls()
    {
        UpdateRankOptionItems();
        UpdateSourceParameterControls();
    }

    private void UpdateSourceParameterControls()
    {
        var template = _pendingTemplate;
        AddConfiguredCardButton.IsEnabled = template is not null;
        SourceParameterTitleTextBlock.Text = template?.Title ?? I18NManager.GetResource(HomePageResources.SourceParametersTitleTextBlockText);
        SourceParameterDescriptionTextBlock.Text = template?.Description ?? I18NManager.GetResource(HomePageResources.SelectCardSourcePromptTextBlockText);

        SetParameterPanelVisibility(template);
        if (template is null)
            return;

        SourceWorkTypeComboBox.SelectedValue = template.WorkType;
        SourceSimpleWorkTypeComboBox.SelectedValue = template.SimpleWorkType;
        SourcePrivacyPolicyComboBox.SelectedValue = template.PrivacyPolicy;
        UpdateRankOptionItems();
        SourceRankOptionComboBox.SelectedValue = SourceSimpleWorkTypeComboBox.SelectedValue is SimpleWorkType.Novel
            ? App.AppViewModel.AppSettings.NovelRankOption
            : App.AppViewModel.AppSettings.IllustrationRankOption;
        SourceUseSpecifiedRankingDateCheckBox.IsChecked = false;
        SourceRankingDatePicker.SelectedDate = MakoClient.RankingMaxDateTime.LocalDateTime;

        switch (template.SourceKind)
        {
            case HomePageCardSourceKind.WorkBookmarks or HomePageCardSourceKind.WorkPosts
                or HomePageCardSourceKind.UserFollowing or HomePageCardSourceKind.UserMyPixiv:
            {
                SourceUserIdTextBox.Text = App.AppViewModel.PixivUid.ToString();
                SourceEntryIdTextBox.Text = "";
                SourceSearchTextBox.Text = "";
                if (template.SourceKind is not HomePageCardSourceKind.WorkBookmarks)
                    SourceTagTextBox.Text = "";
                break;
            }
            case HomePageCardSourceKind.WorkSearch or HomePageCardSourceKind.UserSearch:
                SourceUserIdTextBox.Text = "";
                SourceEntryIdTextBox.Text = "";
                SourceTagTextBox.Text = "";
                break;
            case HomePageCardSourceKind.SingleUser:
                SourceUserIdTextBox.Text = "";
                SourceEntryIdTextBox.Text = "";
                SourceSearchTextBox.Text = "";
                SourceTagTextBox.Text = "";
                break;
            default:
                SourceUserIdTextBox.Text = "";
                SourceEntryIdTextBox.Text = "";
                SourceSearchTextBox.Text = "";
                SourceTagTextBox.Text = "";
                break;
        }
    }

    private void SetParameterPanelVisibility(HomeCardTemplate? template)
    {
        var sourceKind = template?.SourceKind;
        SourceWorkTypePanel.IsVisible = sourceKind is HomePageCardSourceKind.WorkRecommended or HomePageCardSourceKind.WorkNew or HomePageCardSourceKind.WorkPosts;
        SourceSimpleWorkTypePanel.IsVisible = sourceKind is HomePageCardSourceKind.WorkBookmarks or HomePageCardSourceKind.WorkRanking
            or HomePageCardSourceKind.WorkFollowing or HomePageCardSourceKind.WorkSearch;
        SourcePrivacyPolicyPanel.IsVisible = sourceKind is HomePageCardSourceKind.WorkBookmarks or HomePageCardSourceKind.WorkFollowing or HomePageCardSourceKind.UserFollowing;
        SourceRankOptionPanel.IsVisible = sourceKind is HomePageCardSourceKind.WorkRanking;
        SourceRankingDatePanel.IsVisible = sourceKind is HomePageCardSourceKind.WorkRanking;
        SourceUserIdPanel.IsVisible = sourceKind is HomePageCardSourceKind.WorkBookmarks or HomePageCardSourceKind.WorkPosts
            or HomePageCardSourceKind.UserFollowing or HomePageCardSourceKind.UserMyPixiv or HomePageCardSourceKind.SingleUser;
        SourceEntryIdPanel.IsVisible = sourceKind is HomePageCardSourceKind.SingleImage or HomePageCardSourceKind.SingleNovel;
        SourceSearchTextPanel.IsVisible = sourceKind is HomePageCardSourceKind.WorkSearch or HomePageCardSourceKind.UserSearch;
        SourceTagPanel.IsVisible = sourceKind is HomePageCardSourceKind.WorkBookmarks;
    }

    private void UpdateRankOptionItems()
    {
        if (SourceRankOptionComboBox is null || SourceSimpleWorkTypeComboBox is null)
            return;
        if (SourceSimpleWorkTypeComboBox.GetSelectedValue<SimpleWorkType>() is SimpleWorkType.IllustrationAndManga)
        {
            SourceRankOptionComboBox.ItemsSource = SymbolComboBoxItem.GetValues<RankOption>(nameof(Illustration));
            SourceRankOptionComboBox.SelectedValue = App.AppViewModel.AppSettings.IllustrationRankOption;
        }
        else
        {
            SourceRankOptionComboBox.ItemsSource = SymbolComboBoxItem.GetValues<RankOption>(nameof(Novel));
            SourceRankOptionComboBox.SelectedValue = App.AppViewModel.AppSettings.NovelRankOption;
        }
    }

    private HomePageCardLayout? TryCreateCardFromSourceParameters(HomeCardTemplate template)
    {
        var card = CreateCard(template, 0, 0, 1, 1);

        var userId = 0L;
        var entryId = 0L;
        if (NeedsUserId(template.SourceKind) && !TryReadUInt64(SourceUserIdTextBox, out userId))
            return ShowInvalidParameterWarning();

        if (NeedsEntryId(template.SourceKind) && !TryReadUInt64(SourceEntryIdTextBox, out entryId))
            return ShowInvalidParameterWarning();

        if (template.SourceKind is HomePageCardSourceKind.WorkSearch or HomePageCardSourceKind.UserSearch
            && string.IsNullOrWhiteSpace(SourceSearchTextBox.Text))
            return ShowInvalidParameterWarning();

        card.WorkType = SourceWorkTypeComboBox.SelectedValue is WorkType workType ? workType : template.WorkType;
        card.SimpleWorkType = SourceSimpleWorkTypeComboBox.SelectedValue is SimpleWorkType simpleWorkType ? simpleWorkType : template.SimpleWorkType;
        card.TemplateKind = ResolveTemplateKind(template, card.WorkType, card.SimpleWorkType);
        card.PrivacyPolicy = SourcePrivacyPolicyComboBox.SelectedValue is PrivacyPolicy privacyPolicy ? privacyPolicy : template.PrivacyPolicy;
        card.RankOption = SourceRankOptionComboBox.SelectedValue is RankOption rankOption ? rankOption : template.RankOption;
        card.UseSpecifiedRankingDate = SourceUseSpecifiedRankingDateCheckBox.IsChecked is true;
        card.UserId = userId;
        card.EntryId = entryId;
        card.SearchText = string.IsNullOrWhiteSpace(SourceSearchTextBox.Text) ? null : SourceSearchTextBox.Text.Trim();
        card.Tag = string.IsNullOrWhiteSpace(SourceTagTextBox.Text) ? null : SourceTagTextBox.Text.Trim();
        card.RankingDate = card.UseSpecifiedRankingDate
            ? new(SourceRankingDatePicker.SelectedDate ?? MakoClient.RankingMaxDateTime.LocalDateTime)
            : MakoClient.RankingMaxDateTime;
        return card;
    }

    private HomePageCardLayout? ShowInvalidParameterWarning()
    {
        TopLevel.GetTopLevel(this)?.ViewContainer?.ShowWarning(
            I18NManager.GetResource(HomePageResources.InvalidSourceParameterWarningTitle),
            I18NManager.GetResource(HomePageResources.InvalidSourceParameterWarningContent));
        return null;
    }

    private static bool NeedsUserId(HomePageCardSourceKind sourceKind) =>
        sourceKind is HomePageCardSourceKind.WorkBookmarks or HomePageCardSourceKind.WorkPosts
            or HomePageCardSourceKind.UserFollowing or HomePageCardSourceKind.UserMyPixiv or HomePageCardSourceKind.SingleUser;

    private static bool NeedsEntryId(HomePageCardSourceKind sourceKind) =>
        sourceKind is HomePageCardSourceKind.SingleImage or HomePageCardSourceKind.SingleNovel;

    private static bool TryReadUInt64(TextBox textBox, out long value) =>
        long.TryParse(textBox.Text, out value) && value > 0;

    private HomeCardTemplate GetTemplate(HomePageCardLayout card) =>
        NormalizeTemplateForCard(
            _cardTemplates.FirstOrDefault(template => template.SourceKind == card.SourceKind && template.TemplateKind == card.TemplateKind)
            ?? _cardTemplates.FirstOrDefault(template => template.SourceKind == card.SourceKind)
            ?? _cardTemplates[0],
            card);

    private static HomeCardTemplate NormalizeTemplateForCard(HomeCardTemplate template, HomePageCardLayout card) =>
        template.TemplateKind == card.TemplateKind ? template : template with { TemplateKind = card.TemplateKind };

    private static HomePageCardTemplateKind ResolveTemplateKind(HomeCardTemplate template, WorkType workType, SimpleWorkType simpleWorkType) =>
        template.SourceKind switch
        {
            HomePageCardSourceKind.WorkRecommended or HomePageCardSourceKind.WorkNew or HomePageCardSourceKind.WorkPosts
                => workType is WorkType.Novel ? HomePageCardTemplateKind.NovelList : HomePageCardTemplateKind.WorkList,
            HomePageCardSourceKind.WorkBookmarks or HomePageCardSourceKind.WorkRanking or HomePageCardSourceKind.WorkFollowing or HomePageCardSourceKind.WorkSearch
                => simpleWorkType is SimpleWorkType.Novel ? HomePageCardTemplateKind.NovelList : HomePageCardTemplateKind.WorkList,
            _ => template.TemplateKind
        };

    private static int DecimalToZeroBasedInt(decimal? value) => Math.Max(0, DecimalToPositiveInt(value) - 1);

    private static int DecimalToPositiveInt(decimal? value) => Math.Max(1, (int) (value ?? 1));

    private static HomePageCardLayout CreateCard(HomeCardTemplate template, int column, int row, int columnSpan, int rowSpan) => new()
    {
        TemplateKind = template.TemplateKind,
        SourceKind = template.SourceKind,
        WorkType = template.WorkType,
        SimpleWorkType = template.SimpleWorkType,
        PrivacyPolicy = template.PrivacyPolicy,
        RankOption = template.RankOption,
        RankingDate = MakoClient.RankingMaxDateTime,
        Column = column,
        Row = row,
        ColumnSpan = columnSpan,
        RowSpan = rowSpan
    };

    private static void SaveLayout() => AppInfo.SaveSettings(App.AppViewModel.AppSettings);
}
