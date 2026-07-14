// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mako;
using Mako.Global.Enum;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Models.Home;
using Pixeval.Views;
using Pixeval.Views.Home;

namespace Pixeval.ViewModels.Home;

public partial class HomePageViewModel : ViewModelBase
{
    private static AppSettings Settings => App.AppViewModel.AppSettings;

    private readonly IReadOnlyList<HomeCardParameterEditorViewModel> _sourceParameterEditors;

    private readonly HomeCardChoiceParameterEditorViewModel _sourceWorkTypeEditor = new(
        HomeCardParameterKinds.WorkType,
        GetResource(HomePageResources.SourceWorkTypeTextBlockText),
        SymbolComboBoxItem.GetValues<WorkType>(),
        WorkType.Illustration);

    private readonly HomeCardChoiceParameterEditorViewModel _sourceSimpleWorkTypeEditor = new(
        HomeCardParameterKinds.SimpleWorkType,
        GetResource(HomePageResources.SourceSimpleWorkTypeTextBlockText),
        SymbolComboBoxItem.GetValues<SimpleWorkType>(),
        SimpleWorkType.Illustration);

    private readonly HomeCardChoiceParameterEditorViewModel _sourcePrivacyPolicyEditor = new(
        HomeCardParameterKinds.PrivacyPolicy,
        GetResource(HomePageResources.SourcePrivacyPolicyTextBlockText),
        SymbolComboBoxItem.GetValues<PrivacyPolicy>(),
        PrivacyPolicy.Public);

    private readonly HomeCardChoiceParameterEditorViewModel _sourceRankOptionEditor = new(
        HomeCardParameterKinds.RankOption,
        GetResource(HomePageResources.SourceRankOptionTextBlockText),
        SymbolComboBoxItem.GetValues<RankOption>(SimpleWorkType.Illustration),
        RankOption.Day);

    private readonly HomeCardRankingDateParameterEditorViewModel _sourceRankingDateEditor = new(
        GetResource(HomePageResources.SourceRankingDateTextBlockText),
        MakoClient.RankingMaxDateTime.LocalDateTime);

    private readonly HomeCardTextParameterEditorViewModel _sourceUserIdEditor = new(
        HomeCardParameterKinds.UserId,
        GetResource(HomePageResources.SourceUserIdTextBlockText));

    private readonly HomeCardTextParameterEditorViewModel _sourceEntryIdEditor = new(
        HomeCardParameterKinds.EntryId,
        GetResource(HomePageResources.SourceEntryIdTextBlockText));

    private readonly HomeCardTextParameterEditorViewModel _sourceSeriesIdEditor = new(
        HomeCardParameterKinds.SeriesId,
        GetResource(HomePageResources.SourceSeriesIdTextBlockText));

    private readonly HomeCardTextParameterEditorViewModel _sourceSearchTextEditor = new(
        HomeCardParameterKinds.SearchText,
        GetResource(HomePageResources.SourceSearchTextTextBlockText));

    private readonly HomeCardTextParameterEditorViewModel _sourceTagEditor = new(
        HomeCardParameterKinds.Tag,
        GetResource(HomePageResources.SourceTagTextBlockText));

    private HomePageCardLayout? _selectedCard;

    public HomePageViewModel()
    {
        _sourceParameterEditors =
        [
            _sourceWorkTypeEditor,
            _sourceSimpleWorkTypeEditor,
            _sourcePrivacyPolicyEditor,
            _sourceRankOptionEditor,
            _sourceRankingDateEditor,
            _sourceUserIdEditor,
            _sourceEntryIdEditor,
            _sourceSeriesIdEditor,
            _sourceSearchTextEditor,
            _sourceTagEditor
        ];
        _sourceSimpleWorkTypeEditor.ValueChanged += SourceSimpleWorkTypeEditor_OnValueChanged;
        UpdateRankOptionEditor();
        SyncGridEditorValues();
        SyncSelectedCardEditorValues();
    }

    public IReadOnlyList<HomeCardDefinition> CardTemplates => HomeCardDefinitions.All;

    [ObservableProperty] public partial bool IsEditMode { get; set; }

    public int RowCount => decimal.ToInt32(decimal.Clamp(Settings.ApplicationSettings.HomePageRows, HomePage.MinimumGridSize, HomePage.MaximumGridSize));

    public int ColumnCount => decimal.ToInt32(decimal.Clamp(Settings.ApplicationSettings.HomePageColumns, HomePage.MinimumGridSize, HomePage.MaximumGridSize));

    [ObservableProperty] public partial decimal GridColumnsValue { get; set; }

    [ObservableProperty] public partial decimal GridRowsValue { get; set; }

    public bool HasSelectedCard => _selectedCard is not null;

    public string SelectedCardDescription => _selectedCard is { } card
        ? HomeCardDefinitions.BuildTitle(card)
        : I18NManager.GetResource(HomePageResources.NoSelectedCardTextBlockText);

    [ObservableProperty] public partial decimal SelectedColumnValue { get; set; } = 1;

    [ObservableProperty] public partial decimal SelectedRowValue { get; set; } = 1;

    [ObservableProperty] public partial decimal SelectedWidthValue { get; set; } = 1;

    [ObservableProperty] public partial decimal SelectedHeightValue { get; set; } = 1;

    [ObservableProperty] public partial Color SelectedBackgroundColor { get; set; } = Color.FromUInt32(0);

    public HomeCardDefinition? PendingTemplate { get; private set; }

    public bool IsAddingConfiguredCard { get; private set; }

    public string SourceParameterTitle =>
        PendingTemplate?.Title ?? I18NManager.GetResource(HomePageResources.SourceParametersTitleTextBlockText);

    public string SourceParameterDescription =>
        PendingTemplate?.Description ?? I18NManager.GetResource(HomePageResources.SelectCardSourcePromptTextBlockText);

    public bool CanAddConfiguredCard => PendingTemplate is not null && !IsAddingConfiguredCard;

    [ObservableProperty]
    public partial IReadOnlyList<HomeCardParameterEditorViewModel> ActiveParameterEditors { get; private set; } = [];

    public void NotifyGridSizeChanged()
    {
        OnPropertyChanged(nameof(RowCount));
        OnPropertyChanged(nameof(ColumnCount));
    }

    public void SyncGridEditorValues()
    {
        GridRowsValue = RowCount;
        GridColumnsValue = ColumnCount;
    }

    public void SetSelectedCard(HomePageCardLayout? card)
    {
        if (ReferenceEquals(_selectedCard, card))
            return;

        _selectedCard = card;
        OnPropertyChanged(nameof(HasSelectedCard));
        OnPropertyChanged(nameof(SelectedCardDescription));
        SyncSelectedCardEditorValues();
    }

    public void SyncSelectedCardEditorValues()
    {
        var card = _selectedCard;
        SelectedColumnValue = (card?.Column ?? 0) + 1;
        SelectedRowValue = (card?.Row ?? 0) + 1;
        SelectedWidthValue = card?.ColumnSpan ?? 1;
        SelectedHeightValue = card?.RowSpan ?? 1;
        SelectedBackgroundColor = Color.FromUInt32(card?.BackgroundColor ?? 0);
    }

    [RelayCommand]
    public void SelectTemplate(HomeCardDefinition template)
    {
        PendingTemplate = template;
        _sourceWorkTypeEditor.Value = template.WorkType;
        _sourceSimpleWorkTypeEditor.Value = template.SimpleWorkType;
        UpdateRankOptionEditor();
        _sourcePrivacyPolicyEditor.Value = template.PrivacyPolicy;
        _sourceRankingDateEditor.Reset(MakoClient.RankingMaxDateTime.LocalDateTime);

        _sourceUserIdEditor.Text = template.UseCurrentUserAsDefault ? PixevalSettings.MyId.ToString() : "";
        _sourceEntryIdEditor.Text = "";
        _sourceSeriesIdEditor.Text = "";
        if (!template.HasParameter(HomeCardParameterKinds.SearchText))
            _sourceSearchTextEditor.Text = "";
        if (!template.HasParameter(HomeCardParameterKinds.Tag))
            _sourceTagEditor.Text = "";

        ActiveParameterEditors = [.. _sourceParameterEditors.Where(editor => template.HasParameter(editor.Kind))];
        NotifySourceParameterStateChanged();
    }

    public void SetAddingConfiguredCard(bool isAddingConfiguredCard)
    {
        if (IsAddingConfiguredCard == isAddingConfiguredCard)
            return;

        IsAddingConfiguredCard = isAddingConfiguredCard;
        OnPropertyChanged(nameof(IsAddingConfiguredCard));
        OnPropertyChanged(nameof(CanAddConfiguredCard));
    }

    public bool TryCreateCardFromSourceParameters(out HomePageCardLayout? card)
    {
        card = null;
        if (PendingTemplate is not { } template)
            return false;

        var draft = new HomePageCardLayout(template.SourceKind, 0, 0, 1, 1);

        var userId = 0L;
        var entryId = 0L;
        var seriesId = 0L;
        if (template.HasParameter(HomeCardParameterKinds.UserId) && !TryReadUInt64(_sourceUserIdEditor.Text, out userId))
            return false;

        if (template.HasParameter(HomeCardParameterKinds.EntryId) && !TryReadUInt64(_sourceEntryIdEditor.Text, out entryId))
            return false;

        if (template.HasParameter(HomeCardParameterKinds.SeriesId) && !TryReadUInt64(_sourceSeriesIdEditor.Text, out seriesId))
            return false;

        if (template.HasParameter(HomeCardParameterKinds.SearchText) && string.IsNullOrWhiteSpace(_sourceSearchTextEditor.Text))
            return false;

        draft.WorkType = _sourceWorkTypeEditor.GetValue<WorkType>();
        draft.SimpleWorkType = _sourceSimpleWorkTypeEditor.GetValue<SimpleWorkType>();
        draft.PrivacyPolicy = _sourcePrivacyPolicyEditor.GetValue<PrivacyPolicy>();
        draft.RankOption = _sourceRankOptionEditor.GetValue<RankOption>();
        draft.UseSpecifiedRankingDate = _sourceRankingDateEditor.UseSpecifiedDate;
        draft.UserId = userId;
        draft.EntryId = entryId;
        draft.SeriesId = seriesId;
        draft.SearchText = string.IsNullOrWhiteSpace(_sourceSearchTextEditor.Text) ? null : _sourceSearchTextEditor.Text.Trim();
        draft.Tag = string.IsNullOrWhiteSpace(_sourceTagEditor.Text) ? null : _sourceTagEditor.Text.Trim();
        draft.RankingDate = draft.UseSpecifiedRankingDate
            ? new(_sourceRankingDateEditor.SelectedDate)
            : MakoClient.RankingMaxDateTime;
        card = draft;
        return true;
    }

    private void NotifySourceParameterStateChanged()
    {
        OnPropertyChanged(nameof(PendingTemplate));
        OnPropertyChanged(nameof(SourceParameterTitle));
        OnPropertyChanged(nameof(SourceParameterDescription));
        OnPropertyChanged(nameof(CanAddConfiguredCard));
    }

    private void SourceSimpleWorkTypeEditor_OnValueChanged(object? sender, EventArgs e) => UpdateRankOptionEditor();

    private void UpdateRankOptionEditor()
    {
        var simpleWorkType = _sourceSimpleWorkTypeEditor.GetValue<SimpleWorkType>();
        var rankOption = simpleWorkType is SimpleWorkType.Illustration
            ? Settings.SearchSettings.IllustrationRankOption
            : Settings.SearchSettings.NovelRankOption;
        _sourceRankOptionEditor.Reset(SymbolComboBoxItem.GetValues<RankOption>(simpleWorkType), rankOption);
    }

    private static bool TryReadUInt64(string? text, out long value) =>
        long.TryParse(text, out value) && value > 0;

    private static string GetResource(string resource) => I18NManager.GetResource(resource);
}
