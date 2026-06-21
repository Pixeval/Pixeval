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
using Pixeval.Models.Options;
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

    public IReadOnlyList<HomeCardTemplate> CardTemplates { get; } = CreateCardTemplates();

    public bool IsEditMode
    {
        get;
        set
        {
            if (value && IsToolbarHidden)
                value = false;

            SetProperty(ref field, value);
        }
    }

    public bool IsToolbarHidden
    {
        get;
        set
        {
            if (!SetProperty(ref field, value))
                return;

            if (value && IsEditMode)
                IsEditMode = false;

            if (Settings.ApplicationSettings.HideHomePageToolbar == value)
                return;

            Settings.ApplicationSettings.HideHomePageToolbar = value;
            AppInfo.SaveSettings(Settings);
        }
    } = Settings.ApplicationSettings.HideHomePageToolbar;

    public bool IsCardTitleHidden
    {
        get;
        set
        {
            if (!SetProperty(ref field, value))
                return;

            if (Settings.ApplicationSettings.HideHomePageCardTitle == value)
                return;

            Settings.ApplicationSettings.HideHomePageCardTitle = value;
            AppInfo.SaveSettings(Settings);
        }
    } = Settings.ApplicationSettings.HideHomePageCardTitle;

    public int RowCount => decimal.ToInt32(decimal.Clamp(Settings.ApplicationSettings.HomePageRows, HomePage.MinimumGridSize, HomePage.MaximumGridSize));

    public int ColumnCount => decimal.ToInt32(decimal.Clamp(Settings.ApplicationSettings.HomePageColumns, HomePage.MinimumGridSize, HomePage.MaximumGridSize));

    [ObservableProperty]
    public partial decimal GridColumnsValue { get; set; }

    [ObservableProperty]
    public partial decimal GridRowsValue { get; set; }

    public bool HasSelectedCard => _selectedCard is not null;

    public string SelectedCardDescription => _selectedCard?.ToString()
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

    public HomeCardTemplate? PendingTemplate { get; private set; }

    public bool IsAddingConfiguredCard { get; private set; }

    public string SourceParameterTitle =>
        PendingTemplate?.Title ?? I18NManager.GetResource(HomePageResources.SourceParametersTitleTextBlockText);

    public string SourceParameterDescription =>
        PendingTemplate?.Description ?? I18NManager.GetResource(HomePageResources.SelectCardSourcePromptTextBlockText);

    public bool CanAddConfiguredCard => PendingTemplate is not null && !IsAddingConfiguredCard;

    public bool IsSourceWorkTypePanelVisible => PendingTemplate?.SourceKind is HomePageCardSourceKind.WorkRecommended
        or HomePageCardSourceKind.WorkNew or HomePageCardSourceKind.WorkPosts;

    public bool IsSourceSimpleWorkTypePanelVisible => PendingTemplate?.SourceKind is HomePageCardSourceKind.WorkBookmarks
        or HomePageCardSourceKind.WorkRanking or HomePageCardSourceKind.WorkFollowing or HomePageCardSourceKind.WorkSearch;

    public bool IsSourcePrivacyPolicyPanelVisible => PendingTemplate?.SourceKind is HomePageCardSourceKind.WorkBookmarks
        or HomePageCardSourceKind.WorkFollowing or HomePageCardSourceKind.UserFollowing;

    public bool IsSourceRankOptionPanelVisible => PendingTemplate?.SourceKind is HomePageCardSourceKind.WorkRanking;

    public bool IsSourceRankingDatePanelVisible => PendingTemplate?.SourceKind is HomePageCardSourceKind.WorkRanking;

    public bool IsSourceUserIdPanelVisible => PendingTemplate?.SourceKind is HomePageCardSourceKind.WorkBookmarks
        or HomePageCardSourceKind.WorkPosts or HomePageCardSourceKind.UserFollowing or HomePageCardSourceKind.UserMyPixiv
        or HomePageCardSourceKind.SingleUser;

    public bool IsSourceEntryIdPanelVisible => PendingTemplate?.SourceKind is HomePageCardSourceKind.SingleImage
        or HomePageCardSourceKind.SingleNovel;

    public bool IsSourceSearchTextPanelVisible => PendingTemplate?.SourceKind is HomePageCardSourceKind.WorkSearch
        or HomePageCardSourceKind.UserSearch;

    public bool IsSourceTagPanelVisible => PendingTemplate?.SourceKind is HomePageCardSourceKind.WorkBookmarks;

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
    } = SimpleWorkType.IllustrationAndManga;

    [ObservableProperty]
    public partial PrivacyPolicy SelectedSourcePrivacyPolicy { get; set; } = PrivacyPolicy.Public;

    [ObservableProperty]
    public partial RankOption SelectedSourceRankOption { get; set; } = App.AppViewModel.AppSettings.SearchSettings.IllustrationRankOption;

    [ObservableProperty]
    public partial IReadOnlyList<SymbolComboBoxItem> SourceRankOptionItems { get; private set; } = SymbolComboBoxItem.GetValues<RankOption>(SimpleWorkType.IllustrationAndManga);

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
    public void SelectTemplate(HomeCardTemplate template)
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

        switch (template.SourceKind)
        {
            case HomePageCardSourceKind.WorkBookmarks or HomePageCardSourceKind.WorkPosts
                or HomePageCardSourceKind.UserFollowing or HomePageCardSourceKind.UserMyPixiv:
            {
                SourceUserIdText = App.AppViewModel.PixivUid.ToString();
                SourceEntryIdText = "";
                SourceSearchText = "";
                if (template.SourceKind is not HomePageCardSourceKind.WorkBookmarks)
                    SourceTagText = "";
                break;
            }
            case HomePageCardSourceKind.WorkSearch or HomePageCardSourceKind.UserSearch:
                SourceUserIdText = "";
                SourceEntryIdText = "";
                SourceTagText = "";
                break;
            case HomePageCardSourceKind.SingleUser:
                SourceUserIdText = "";
                SourceEntryIdText = "";
                SourceSearchText = "";
                SourceTagText = "";
                break;
            default:
                SourceUserIdText = "";
                SourceEntryIdText = "";
                SourceSearchText = "";
                SourceTagText = "";
                break;
        }
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
        if (NeedsUserId(template.SourceKind) && !TryReadUInt64(SourceUserIdText, out userId))
            return false;

        if (NeedsEntryId(template.SourceKind) && !TryReadUInt64(SourceEntryIdText, out entryId))
            return false;

        if (template.SourceKind is HomePageCardSourceKind.WorkSearch or HomePageCardSourceKind.UserSearch
            && string.IsNullOrWhiteSpace(SourceSearchText))
        {
            return false;
        }

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

    public HomeCardTemplate GetTemplate(HomePageCardLayout card) =>
        CardTemplates.FirstOrDefault(template => template.SourceKind == card.SourceKind) ?? CardTemplates[0];

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
        SelectedSourceRankOption = selectedSimpleWorkType is SimpleWorkType.IllustrationAndManga
            ? Settings.SearchSettings.IllustrationRankOption
            : Settings.SearchSettings.NovelRankOption;
    }

    private static bool NeedsUserId(HomePageCardSourceKind sourceKind) =>
        sourceKind is HomePageCardSourceKind.WorkBookmarks or HomePageCardSourceKind.WorkPosts
            or HomePageCardSourceKind.UserFollowing or HomePageCardSourceKind.UserMyPixiv or HomePageCardSourceKind.SingleUser;

    private static bool NeedsEntryId(HomePageCardSourceKind sourceKind) =>
        sourceKind is HomePageCardSourceKind.SingleImage or HomePageCardSourceKind.SingleNovel;

    private static bool TryReadUInt64(string? text, out long value) =>
        long.TryParse(text, out value) && value > 0;

    private static IReadOnlyList<HomeCardTemplate> CreateCardTemplates() =>
    [
        new(HomePageCardSourceKind.WorkRecommended),
        new(HomePageCardSourceKind.WorkBookmarks),
        new(HomePageCardSourceKind.WorkRanking),
        new(HomePageCardSourceKind.WorkNew),
        new(HomePageCardSourceKind.WorkFollowing),
        new(HomePageCardSourceKind.WorkPosts),
        new(HomePageCardSourceKind.WorkSearch),
        new(HomePageCardSourceKind.UserRecommended),
        new(HomePageCardSourceKind.UserSearch),
        new(HomePageCardSourceKind.UserFollowing),
        new(HomePageCardSourceKind.UserMyPixiv),
        new(HomePageCardSourceKind.Spotlight),
        new(HomePageCardSourceKind.SingleImage),
        new(HomePageCardSourceKind.SingleNovel, WorkType.Novel, SimpleWorkType.Novel),
        new(HomePageCardSourceKind.SingleUser)
    ];
}
