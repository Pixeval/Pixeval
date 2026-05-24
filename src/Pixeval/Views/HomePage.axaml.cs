// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Media;
using Pixeval.I18N;
using Pixeval.Models.Home;
using Pixeval.Views.Home;

namespace Pixeval.Views;

public partial class HomePage : ContentPage
{
    private const int MinimumGridSize = 1;
    private const int MaximumGridSize = 12;

    private readonly ObservableCollection<HomePageCardLayout> _cards;
    private readonly IReadOnlyList<HomeCardTemplate> _cardTemplates;
    private bool _isUpdatingGridSizeControls;
    private bool _isUpdatingSelectedCardControls;
    private HomePageCardControl? _activeCardControl;
    private HomePageCardLayout? _selectedCard;

    public HomePage()
    {
        InitializeComponent();

        _cards = App.AppViewModel.AppSettings.HomePageCards;
        _cardTemplates = CreateCardTemplates();
        CardLibraryItemsControl.ItemsSource = _cardTemplates;
        UpdateGridSizeControls();

        if (!App.AppViewModel.AppSettings.IsHomePageLayoutInitialized)
        {
            CreateDefaultCards();
            App.AppViewModel.AppSettings.IsHomePageLayoutInitialized = true;
            SaveLayout();
        }

        RefreshGrid();
    }

    private int RowCount => Math.Clamp(App.AppViewModel.AppSettings.HomePageRows, MinimumGridSize, MaximumGridSize);

    private int ColumnCount => Math.Clamp(App.AppViewModel.AppSettings.HomePageColumns, MinimumGridSize, MaximumGridSize);

    private static IReadOnlyList<HomeCardTemplate> CreateCardTemplates() =>
    [
        new(HomePageCardKind.RecommendedWorks, I18NManager.GetResource(HomePageResources.RecommendedWorksCardTitle), I18NManager.GetResource(HomePageResources.RecommendedWorksCardDescription), 3, 2, Color.FromArgb(0xFF, 0x4C, 0xA7, 0xF5)),
        new(HomePageCardKind.RecommendedUsers, I18NManager.GetResource(HomePageResources.RecommendedUsersCardTitle), I18NManager.GetResource(HomePageResources.RecommendedUsersCardDescription), 2, 2, Color.FromArgb(0xFF, 0x69, 0xC2, 0x8F)),
        new(HomePageCardKind.RecommendedNovels, I18NManager.GetResource(HomePageResources.RecommendedNovelsCardTitle), I18NManager.GetResource(HomePageResources.RecommendedNovelsCardDescription), 2, 2, Color.FromArgb(0xFF, 0xB3, 0x85, 0xF2)),
        new(HomePageCardKind.RankingWorks, I18NManager.GetResource(HomePageResources.RankingWorksCardTitle), I18NManager.GetResource(HomePageResources.RankingWorksCardDescription), 3, 2, Color.FromArgb(0xFF, 0xF7, 0xB8, 0x4B)),
        new(HomePageCardKind.Spotlights, I18NManager.GetResource(HomePageResources.SpotlightsCardTitle), I18NManager.GetResource(HomePageResources.SpotlightsCardDescription), 2, 3, Color.FromArgb(0xFF, 0xEF, 0x6F, 0x88)),
        new(HomePageCardKind.SingleImage, I18NManager.GetResource(HomePageResources.SingleImageCardTitle), I18NManager.GetResource(HomePageResources.SingleImageCardDescription), 2, 2, Color.FromArgb(0xFF, 0x5B, 0xC8, 0xD7))
    ];
}
