// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Media;
using Mako.Global.Enum;
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
    private HomeCardTemplate? _pendingTemplate;
    private bool _isRefreshingGrid;
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
        InitializeSourceParameterControls();

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
        new(HomePageCardSourceKind.WorkRecommended, HomePageCardTemplateKind.WorkList, I18NManager.GetResource(HomePageResources.WorkRecommendedCardTitle), I18NManager.GetResource(HomePageResources.WorkRecommendedCardDescription), 3, 2, Color.FromArgb(0xFF, 0x4C, 0xA7, 0xF5)),
        new(HomePageCardSourceKind.WorkBookmarks, HomePageCardTemplateKind.WorkList, I18NManager.GetResource(HomePageResources.WorkBookmarksCardTitle), I18NManager.GetResource(HomePageResources.WorkBookmarksCardDescription), 3, 2, Color.FromArgb(0xFF, 0x53, 0xB8, 0xA9)),
        new(HomePageCardSourceKind.WorkRanking, HomePageCardTemplateKind.WorkList, I18NManager.GetResource(HomePageResources.WorkRankingCardTitle), I18NManager.GetResource(HomePageResources.WorkRankingCardDescription), 3, 2, Color.FromArgb(0xFF, 0xF7, 0xB8, 0x4B)),
        new(HomePageCardSourceKind.WorkNew, HomePageCardTemplateKind.WorkList, I18NManager.GetResource(HomePageResources.WorkNewCardTitle), I18NManager.GetResource(HomePageResources.WorkNewCardDescription), 3, 2, Color.FromArgb(0xFF, 0x47, 0x96, 0xC8)),
        new(HomePageCardSourceKind.WorkFollowing, HomePageCardTemplateKind.WorkList, I18NManager.GetResource(HomePageResources.WorkFollowingCardTitle), I18NManager.GetResource(HomePageResources.WorkFollowingCardDescription), 3, 2, Color.FromArgb(0xFF, 0x8C, 0xB3, 0x5F)),
        new(HomePageCardSourceKind.WorkPosts, HomePageCardTemplateKind.WorkList, I18NManager.GetResource(HomePageResources.WorkPostsCardTitle), I18NManager.GetResource(HomePageResources.WorkPostsCardDescription), 3, 2, Color.FromArgb(0xFF, 0x4F, 0x86, 0xF0)),
        new(HomePageCardSourceKind.WorkSearch, HomePageCardTemplateKind.WorkList, I18NManager.GetResource(HomePageResources.WorkSearchCardTitle), I18NManager.GetResource(HomePageResources.WorkSearchCardDescription), 3, 2, Color.FromArgb(0xFF, 0xC0, 0x71, 0x9D)),
        new(HomePageCardSourceKind.UserRecommended, HomePageCardTemplateKind.UserList, I18NManager.GetResource(HomePageResources.UserRecommendedCardTitle), I18NManager.GetResource(HomePageResources.UserRecommendedCardDescription), 2, 2, Color.FromArgb(0xFF, 0x69, 0xC2, 0x8F)),
        new(HomePageCardSourceKind.UserSearch, HomePageCardTemplateKind.UserList, I18NManager.GetResource(HomePageResources.UserSearchCardTitle), I18NManager.GetResource(HomePageResources.UserSearchCardDescription), 2, 2, Color.FromArgb(0xFF, 0x78, 0x94, 0xD6)),
        new(HomePageCardSourceKind.UserFollowing, HomePageCardTemplateKind.UserList, I18NManager.GetResource(HomePageResources.UserFollowingCardTitle), I18NManager.GetResource(HomePageResources.UserFollowingCardDescription), 2, 2, Color.FromArgb(0xFF, 0x52, 0xB4, 0x6D)),
        new(HomePageCardSourceKind.UserMyPixiv, HomePageCardTemplateKind.UserList, I18NManager.GetResource(HomePageResources.UserMyPixivCardTitle), I18NManager.GetResource(HomePageResources.UserMyPixivCardDescription), 2, 2, Color.FromArgb(0xFF, 0x8D, 0x77, 0xC8)),
        new(HomePageCardSourceKind.Spotlight, HomePageCardTemplateKind.SpotlightList, I18NManager.GetResource(HomePageResources.SpotlightsCardTitle), I18NManager.GetResource(HomePageResources.SpotlightsCardDescription), 2, 3, Color.FromArgb(0xFF, 0xEF, 0x6F, 0x88)),
        new(HomePageCardSourceKind.SingleImage, HomePageCardTemplateKind.SingleImage, I18NManager.GetResource(HomePageResources.SingleImageCardTitle), I18NManager.GetResource(HomePageResources.SingleImageCardDescription), 2, 2, Color.FromArgb(0xFF, 0x5B, 0xC8, 0xD7)),
        new(HomePageCardSourceKind.SingleNovel, HomePageCardTemplateKind.SingleNovel, I18NManager.GetResource(HomePageResources.SingleNovelCardTitle), I18NManager.GetResource(HomePageResources.SingleNovelCardDescription), 2, 2, Color.FromArgb(0xFF, 0xB3, 0x85, 0xF2), WorkType.Novel, SimpleWorkType.Novel),
        new(HomePageCardSourceKind.SingleUser, HomePageCardTemplateKind.SingleUser, I18NManager.GetResource(HomePageResources.SingleUserCardTitle), I18NManager.GetResource(HomePageResources.SingleUserCardDescription), 2, 2, Color.FromArgb(0xFF, 0xDF, 0x7D, 0x5C))
    ];
}
