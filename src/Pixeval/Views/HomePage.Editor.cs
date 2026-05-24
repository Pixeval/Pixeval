// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using FluentIcons.Common;
using Mako;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Models.Home;
using Pixeval.Models.Settings;
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
        RefreshEditModeVisuals();
    }

    private void ResetLayoutButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _cards.Clear();
        SelectCard(null);
        CreateDefaultCards();
        SaveLayout();
        RefreshGrid();
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
        if (_pendingTemplate is not { } template || !TryCreateCardFromSourceParameters(template, out var draft))
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

    private void SourceSimpleWorkTypeComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e)
    {
        UpdateRankOptionItems();
    }

    private void DeleteSelectedCardButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_selectedCard is null)
            return;

        var deletedCard = _selectedCard;
        _ = _cards.Remove(deletedCard);
        RemoveCardControl(deletedCard);
        SelectCard(null);
        SaveLayout();
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
        UpdateSelectedCardLayoutVisual();
        UpdateSelectedCardControls();
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

    private void SelectCard(HomePageCardLayout? card)
    {
        _selectedCard = card;
        DeleteSelectedCardButton.IsEnabled = card is not null;
        SelectedCardTextBlock.Text = card is null
            ? I18NManager.GetResource(HomePageResources.NoSelectedCardTextBlockText)
            : GetTemplate(card).Title;
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

    private void InitializeSourceParameterControls()
    {
        RegisterHomePageEnums();
        SourceWorkTypeComboBox.ItemsSource = SymbolComboBoxItem.GetValues<WorkType>();
        SourceSimpleWorkTypeComboBox.ItemsSource = SymbolComboBoxItem.GetValues<SimpleWorkType>();
        SourcePrivacyPolicyComboBox.ItemsSource = SymbolComboBoxItem.GetValues<PrivacyPolicy>();
        SourceSpotlightCategoryComboBox.ItemsSource = SymbolComboBoxItem.GetValues<SpotlightCategory>();
        SourceRankingDatePicker.DisplayDateEnd = MakoClient.RankingMaxDateTime.LocalDateTime;
        SourceRankingDatePicker.SelectedDate = MakoClient.RankingMaxDateTime.LocalDateTime;
        UpdateRankOptionItems();
        UpdateSourceParameterControls();
    }

    private static void RegisterHomePageEnums()
    {
        if (!LocalSettingsEntryHelper.RegisteredAttach.ContainsKey(typeof(SpotlightCategory)))
        {
            LocalSettingsEntryHelper.RegisterAttach<SpotlightCategory>(t =>
            {
                t.Register(SpotlightCategory.All, Symbol.SlideTextSparkle, HomePageResources.SpotlightCategoryAll);
                t.Register(SpotlightCategory.Spotlight, Symbol.SlideTextSparkle, HomePageResources.SpotlightCategorySpotlight);
                t.Register(SpotlightCategory.Tutorial, Symbol.BookQuestionMark, HomePageResources.SpotlightCategoryTutorial);
                t.Register(SpotlightCategory.Inspiration, Symbol.DesignIdeas, HomePageResources.SpotlightCategoryInspiration);
            });
        }
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
        SourceSpotlightCategoryComboBox.SelectedValue = template.SpotlightCategory;
        UpdateRankOptionItems();
        SourceRankOptionComboBox.SelectedValue = SourceSimpleWorkTypeComboBox.SelectedValue is SimpleWorkType.Novel
            ? App.AppViewModel.AppSettings.NovelRankOption
            : App.AppViewModel.AppSettings.IllustrationRankOption;
        SourceRankingDatePicker.SelectedDate = MakoClient.RankingMaxDateTime.LocalDateTime;

        if (template.SourceKind is HomePageCardSourceKind.WorkBookmarks or HomePageCardSourceKind.WorkPosts
            or HomePageCardSourceKind.UserFollowing or HomePageCardSourceKind.UserMyPixiv)
        {
            SourceUserIdTextBox.Text = App.AppViewModel.PixivUid.ToString();
            SourceEntryIdTextBox.Text = "";
            SourceSearchTextBox.Text = "";
            if (template.SourceKind is not HomePageCardSourceKind.WorkBookmarks)
                SourceTagTextBox.Text = "";
        }
        else if (template.SourceKind is HomePageCardSourceKind.WorkSearch or HomePageCardSourceKind.UserSearch)
        {
            SourceUserIdTextBox.Text = "";
            SourceEntryIdTextBox.Text = "";
            SourceTagTextBox.Text = "";
        }
        else if (template.SourceKind is HomePageCardSourceKind.SingleUser)
        {
            SourceUserIdTextBox.Text = "";
            SourceEntryIdTextBox.Text = "";
            SourceSearchTextBox.Text = "";
            SourceTagTextBox.Text = "";
        }
        else
        {
            SourceUserIdTextBox.Text = "";
            SourceEntryIdTextBox.Text = "";
            SourceSearchTextBox.Text = "";
            SourceTagTextBox.Text = "";
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
        SourceSpotlightCategoryPanel.IsVisible = sourceKind is HomePageCardSourceKind.Spotlight;
    }

    private void UpdateRankOptionItems()
    {
        if (SourceRankOptionComboBox is null || SourceSimpleWorkTypeComboBox is null)
            return;

        var key = SourceSimpleWorkTypeComboBox.SelectedValue is SimpleWorkType.Novel ? "Novel" : "Illustration";
        SourceRankOptionComboBox.ItemsSource = SymbolComboBoxItem.GetValues(key);
        SourceRankOptionComboBox.SelectedValue = SourceSimpleWorkTypeComboBox.SelectedValue is SimpleWorkType.Novel
            ? App.AppViewModel.AppSettings.NovelRankOption
            : App.AppViewModel.AppSettings.IllustrationRankOption;
    }

    private bool TryCreateCardFromSourceParameters(HomeCardTemplate template, out HomePageCardLayout card)
    {
        card = CreateCard(template, 0, 0, 1, 1);

        var userId = 0L;
        var entryId = 0L;
        if (NeedsUserId(template.SourceKind) && !TryReadPositiveInt64(SourceUserIdTextBox, out userId))
            return ShowInvalidParameterWarning(out card);

        if (NeedsEntryId(template.SourceKind) && !TryReadPositiveInt64(SourceEntryIdTextBox, out entryId))
            return ShowInvalidParameterWarning(out card);

        if (template.SourceKind is HomePageCardSourceKind.WorkSearch or HomePageCardSourceKind.UserSearch
            && string.IsNullOrWhiteSpace(SourceSearchTextBox.Text))
            return ShowInvalidParameterWarning(out card);

        card.WorkType = SourceWorkTypeComboBox.SelectedValue is WorkType workType ? workType : template.WorkType;
        card.SimpleWorkType = SourceSimpleWorkTypeComboBox.SelectedValue is SimpleWorkType simpleWorkType ? simpleWorkType : template.SimpleWorkType;
        card.TemplateKind = ResolveTemplateKind(template, card.WorkType, card.SimpleWorkType);
        card.PrivacyPolicy = SourcePrivacyPolicyComboBox.SelectedValue is PrivacyPolicy privacyPolicy ? privacyPolicy : template.PrivacyPolicy;
        card.RankOption = SourceRankOptionComboBox.SelectedValue is RankOption rankOption ? rankOption : template.RankOption;
        card.SpotlightCategory = SourceSpotlightCategoryComboBox.SelectedValue is SpotlightCategory spotlightCategory ? spotlightCategory : template.SpotlightCategory;
        card.UserId = userId;
        card.EntryId = entryId;
        card.SearchText = string.IsNullOrWhiteSpace(SourceSearchTextBox.Text) ? null : SourceSearchTextBox.Text.Trim();
        card.Tag = string.IsNullOrWhiteSpace(SourceTagTextBox.Text) ? null : SourceTagTextBox.Text.Trim();
        card.RankingDate = new(SourceRankingDatePicker.SelectedDate ?? MakoClient.RankingMaxDateTime.LocalDateTime);
        return true;
    }

    private bool ShowInvalidParameterWarning(out HomePageCardLayout card)
    {
        card = new();
        TopLevel.GetTopLevel(this)?.ViewContainer?.ShowWarning(
            I18NManager.GetResource(HomePageResources.InvalidSourceParameterWarningTitle),
            I18NManager.GetResource(HomePageResources.InvalidSourceParameterWarningContent));
        return false;
    }

    private static bool NeedsUserId(HomePageCardSourceKind sourceKind) =>
        sourceKind is HomePageCardSourceKind.WorkBookmarks or HomePageCardSourceKind.WorkPosts
            or HomePageCardSourceKind.UserFollowing or HomePageCardSourceKind.UserMyPixiv or HomePageCardSourceKind.SingleUser;

    private static bool NeedsEntryId(HomePageCardSourceKind sourceKind) =>
        sourceKind is HomePageCardSourceKind.SingleImage or HomePageCardSourceKind.SingleNovel;

    private static bool TryReadPositiveInt64(TextBox textBox, out long value) =>
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
        SpotlightCategory = template.SpotlightCategory,
        RankingDate = MakoClient.RankingMaxDateTime,
        Column = column,
        Row = row,
        ColumnSpan = columnSpan,
        RowSpan = rowSpan
    };

    private static void SaveLayout() => AppInfo.SaveSettings(App.AppViewModel.AppSettings);
}
