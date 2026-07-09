// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
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

    private HomePageCardLayout? _selectedCard;

    public HomePageViewModel()
    {
        SyncGridEditorValues();
        SyncSelectedCardEditorValues();
    }

    public IReadOnlyList<HomeCardDefinition> CardTemplates => HomeCardDefinitions.All;

    [ObservableProperty]
    public partial bool IsEditMode { get; set; }

    public int RowCount => decimal.ToInt32(decimal.Clamp(Settings.ApplicationSettings.HomePageRows, HomePage.MinimumGridSize, HomePage.MaximumGridSize));

    public int ColumnCount => decimal.ToInt32(decimal.Clamp(Settings.ApplicationSettings.HomePageColumns, HomePage.MinimumGridSize, HomePage.MaximumGridSize));

    [ObservableProperty]
    public partial decimal GridColumnsValue { get; set; }

    [ObservableProperty]
    public partial decimal GridRowsValue { get; set; }

    public bool HasSelectedCard => _selectedCard is not null;

    public string SelectedCardDescription => _selectedCard?.BuildTitle()
                                             ?? I18NManager.GetResource(HomePageResources.NoSelectedCardTextBlockText);

    [ObservableProperty]
    public partial decimal SelectedColumnValue { get; set; } = 1;

    [ObservableProperty]
    public partial decimal SelectedRowValue { get; set; } = 1;

    [ObservableProperty]
    public partial decimal SelectedWidthValue { get; set; } = 1;

    [ObservableProperty]
    public partial decimal SelectedHeightValue { get; set; } = 1;

    [ObservableProperty]
    public partial Color SelectedBackgroundColor { get; set; } = Color.FromUInt32(0);

    public HomeCardDefinition? PendingTemplate { get; private set; }

    public bool IsAddingConfiguredCard { get; private set; }

    public string SourceParameterTitle =>
        PendingTemplate?.Title ?? I18NManager.GetResource(HomePageResources.SourceParametersTitleTextBlockText);

    public string SourceParameterDescription =>
        PendingTemplate?.Description ?? I18NManager.GetResource(HomePageResources.SelectCardSourcePromptTextBlockText);

    public bool CanAddConfiguredCard => PendingTemplate is not null && !IsAddingConfiguredCard;

    public bool IsSourceWorkTypePanelVisible => HasPendingParameter(HomeCardParameterKinds.WorkType);

    public bool IsSourceSimpleWorkTypePanelVisible => HasPendingParameter(HomeCardParameterKinds.SimpleWorkType);

    public bool IsSourcePrivacyPolicyPanelVisible => HasPendingParameter(HomeCardParameterKinds.PrivacyPolicy);

    public bool IsSourceRankOptionPanelVisible => HasPendingParameter(HomeCardParameterKinds.RankOption);

    public bool IsSourceRankingDatePanelVisible => HasPendingParameter(HomeCardParameterKinds.RankingDate);

    public bool IsSourceUserIdPanelVisible => HasPendingParameter(HomeCardParameterKinds.UserId);

    public bool IsSourceEntryIdPanelVisible => HasPendingParameter(HomeCardParameterKinds.EntryId);

    public bool IsSourceSearchTextPanelVisible => HasPendingParameter(HomeCardParameterKinds.SearchText);

    public bool IsSourceTagPanelVisible => HasPendingParameter(HomeCardParameterKinds.Tag);

    [ObservableProperty]
    public partial WorkType SelectedSourceWorkType { get; set; } = WorkType.Illustration;

    public SimpleWorkType SelectedSourceSimpleWorkType
    {
        get;
        set
        {
            if (!SetProperty(ref field, value))
                return;

            UpdateRankOptionItems();
        }
    } = SimpleWorkType.Illustration;

    [ObservableProperty]
    public partial PrivacyPolicy SelectedSourcePrivacyPolicy { get; set; } = PrivacyPolicy.Public;

    [ObservableProperty]
    public partial RankOption SelectedSourceRankOption { get; set; } = App.AppViewModel.AppSettings.SearchSettings.IllustrationRankOption;

    [ObservableProperty]
    public partial IReadOnlyList<SymbolComboBoxItem> SourceRankOptionItems { get; private set; } = SymbolComboBoxItem.GetValues<RankOption>(SimpleWorkType.Illustration);

    [ObservableProperty]
    public partial bool UseSpecifiedRankingDate { get; set; }

    [ObservableProperty]
    public partial DateTime SelectedRankingDate { get; set; } = MakoClient.RankingMaxDateTime.LocalDateTime;

    [ObservableProperty]
    public partial string SourceUserIdText { get; set; } = "";

    [ObservableProperty]
    public partial string SourceEntryIdText { get; set; } = "";

    [ObservableProperty]
    public partial string SourceSearchText { get; set; } = "";

    [ObservableProperty]
    public partial string SourceTagText { get; set; } = "";

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
        NotifySourceParameterStateChanged();

        SelectedSourceWorkType = template.WorkType;
        SelectedSourceSimpleWorkType = template.SimpleWorkType;
        SelectedSourcePrivacyPolicy = template.PrivacyPolicy;
        SelectedSourceRankOption = SelectedSourceSimpleWorkType is SimpleWorkType.Novel
            ? Settings.SearchSettings.NovelRankOption
            : Settings.SearchSettings.IllustrationRankOption;
        UseSpecifiedRankingDate = false;
        SelectedRankingDate = MakoClient.RankingMaxDateTime.LocalDateTime;

        SourceUserIdText = template.UseCurrentUserAsDefault ? PixevalSettings.MyId.ToString() : "";
        SourceEntryIdText = "";
        if (!template.HasParameter(HomeCardParameterKinds.SearchText))
            SourceSearchText = "";
        if (!template.HasParameter(HomeCardParameterKinds.Tag))
            SourceTagText = "";
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

        var draft = new HomePageCardLayout(template, 0, 0, 1, 1);

        var userId = 0L;
        var entryId = 0L;
        if (template.HasParameter(HomeCardParameterKinds.UserId) && !TryReadUInt64(SourceUserIdText, out userId))
            return false;

        if (template.HasParameter(HomeCardParameterKinds.EntryId) && !TryReadUInt64(SourceEntryIdText, out entryId))
            return false;

        if (template.HasParameter(HomeCardParameterKinds.SearchText) && string.IsNullOrWhiteSpace(SourceSearchText))
            return false;

        draft.WorkType = SelectedSourceWorkType;
        draft.SimpleWorkType = SelectedSourceSimpleWorkType;
        draft.PrivacyPolicy = SelectedSourcePrivacyPolicy;
        draft.RankOption = SelectedSourceRankOption;
        draft.UseSpecifiedRankingDate = UseSpecifiedRankingDate;
        draft.UserId = userId;
        draft.EntryId = entryId;
        draft.SearchText = string.IsNullOrWhiteSpace(SourceSearchText) ? null : SourceSearchText.Trim();
        draft.Tag = string.IsNullOrWhiteSpace(SourceTagText) ? null : SourceTagText.Trim();
        draft.RankingDate = draft.UseSpecifiedRankingDate
            ? new(SelectedRankingDate)
            : MakoClient.RankingMaxDateTime;
        card = draft;
        return true;
    }

    public HomeCardDefinition GetDefinition(HomePageCardLayout card) =>
        HomeCardDefinitions.Get(card.SourceKind);

    private void NotifySourceParameterStateChanged()
    {
        OnPropertyChanged(nameof(PendingTemplate));
        OnPropertyChanged(nameof(SourceParameterTitle));
        OnPropertyChanged(nameof(SourceParameterDescription));
        OnPropertyChanged(nameof(CanAddConfiguredCard));
        OnPropertyChanged(nameof(IsSourceWorkTypePanelVisible));
        OnPropertyChanged(nameof(IsSourceSimpleWorkTypePanelVisible));
        OnPropertyChanged(nameof(IsSourcePrivacyPolicyPanelVisible));
        OnPropertyChanged(nameof(IsSourceRankOptionPanelVisible));
        OnPropertyChanged(nameof(IsSourceRankingDatePanelVisible));
        OnPropertyChanged(nameof(IsSourceUserIdPanelVisible));
        OnPropertyChanged(nameof(IsSourceEntryIdPanelVisible));
        OnPropertyChanged(nameof(IsSourceSearchTextPanelVisible));
        OnPropertyChanged(nameof(IsSourceTagPanelVisible));
    }

    private void UpdateRankOptionItems()
    {
        var selectedSimpleWorkType = SelectedSourceSimpleWorkType;
        SourceRankOptionItems = SymbolComboBoxItem.GetValues<RankOption>(selectedSimpleWorkType);
        SelectedSourceRankOption = selectedSimpleWorkType is SimpleWorkType.Illustration
            ? Settings.SearchSettings.IllustrationRankOption
            : Settings.SearchSettings.NovelRankOption;
    }

    private bool HasPendingParameter(HomeCardParameterKinds parameter) =>
        PendingTemplate?.HasParameter(parameter) is true;

    private static bool TryReadUInt64(string? text, out long value) =>
        long.TryParse(text, out value) && value > 0;

}
