// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using FluentIcons.Common;
using Mako.Global.Enum;
using Pixeval.I18N;
using Pixeval.Models.Home;

namespace Pixeval.Views.Home;

public partial class HomePage : ContentPage
{
    public const decimal MinimumGridSize = 1;
    public const decimal MaximumGridSize = 12;

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
        InitializeDisplaySettingsControls();

        RefreshGrid();
        ApplyDisplaySettings();
    }

    private int RowCount => decimal.ToInt32(Math.Clamp(App.AppViewModel.AppSettings.HomePageRows, MinimumGridSize, MaximumGridSize));

    private int ColumnCount => decimal.ToInt32(Math.Clamp(App.AppViewModel.AppSettings.HomePageColumns, MinimumGridSize, MaximumGridSize));

    private static IReadOnlyList<HomeCardTemplate> CreateCardTemplates() =>
    [
        new(HomePageCardSourceKind.WorkRecommended, HomePageCardTemplateKind.WorkList, Symbol.Calendar, I18NManager.GetResource(HomePageResources.WorkRecommendedCardTitle), I18NManager.GetResource(HomePageResources.WorkRecommendedCardDescription)),
        new(HomePageCardSourceKind.WorkBookmarks, HomePageCardTemplateKind.WorkList, Symbol.Library, I18NManager.GetResource(HomePageResources.WorkBookmarksCardTitle), I18NManager.GetResource(HomePageResources.WorkBookmarksCardDescription)),
        new(HomePageCardSourceKind.WorkRanking, HomePageCardTemplateKind.WorkList, Symbol.ArrowTrendingLines, I18NManager.GetResource(HomePageResources.WorkRankingCardTitle), I18NManager.GetResource(HomePageResources.WorkRankingCardDescription)),
        new(HomePageCardSourceKind.WorkNew, HomePageCardTemplateKind.WorkList, Symbol.ArrowSync, I18NManager.GetResource(HomePageResources.WorkNewCardTitle), I18NManager.GetResource(HomePageResources.WorkNewCardDescription)),
        new(HomePageCardSourceKind.WorkFollowing, HomePageCardTemplateKind.WorkList, Symbol.PersonHeart, I18NManager.GetResource(HomePageResources.WorkFollowingCardTitle), I18NManager.GetResource(HomePageResources.WorkFollowingCardDescription)),
        new(HomePageCardSourceKind.WorkPosts, HomePageCardTemplateKind.WorkList, Symbol.AlertUrgent, I18NManager.GetResource(HomePageResources.WorkPostsCardTitle), I18NManager.GetResource(HomePageResources.WorkPostsCardDescription)),
        new(HomePageCardSourceKind.WorkSearch, HomePageCardTemplateKind.WorkList, Symbol.SearchSparkle, I18NManager.GetResource(HomePageResources.WorkSearchCardTitle), I18NManager.GetResource(HomePageResources.WorkSearchCardDescription)),
        new(HomePageCardSourceKind.UserRecommended, HomePageCardTemplateKind.UserList, Symbol.PeopleCommunity, I18NManager.GetResource(HomePageResources.UserRecommendedCardTitle), I18NManager.GetResource(HomePageResources.UserRecommendedCardDescription)),
        new(HomePageCardSourceKind.UserSearch, HomePageCardTemplateKind.UserList, Symbol.SearchSparkle, I18NManager.GetResource(HomePageResources.UserSearchCardTitle), I18NManager.GetResource(HomePageResources.UserSearchCardDescription)),
        new(HomePageCardSourceKind.UserFollowing, HomePageCardTemplateKind.UserList, Symbol.PersonHeart, I18NManager.GetResource(HomePageResources.UserFollowingCardTitle), I18NManager.GetResource(HomePageResources.UserFollowingCardDescription)),
        new(HomePageCardSourceKind.UserMyPixiv, HomePageCardTemplateKind.UserList, Symbol.People, I18NManager.GetResource(HomePageResources.UserMyPixivCardTitle), I18NManager.GetResource(HomePageResources.UserMyPixivCardDescription)),
        new(HomePageCardSourceKind.Spotlight, HomePageCardTemplateKind.SpotlightList, Symbol.SlideTextSparkle, I18NManager.GetResource(HomePageResources.SpotlightsCardTitle), I18NManager.GetResource(HomePageResources.SpotlightsCardDescription)),
        new(HomePageCardSourceKind.SingleImage, HomePageCardTemplateKind.SingleImage, Symbol.Image, I18NManager.GetResource(HomePageResources.SingleImageCardTitle), I18NManager.GetResource(HomePageResources.SingleImageCardDescription)),
        new(HomePageCardSourceKind.SingleNovel, HomePageCardTemplateKind.SingleNovel, Symbol.BookOpen, I18NManager.GetResource(HomePageResources.SingleNovelCardTitle), I18NManager.GetResource(HomePageResources.SingleNovelCardDescription), WorkType.Novel, SimpleWorkType.Novel),
        new(HomePageCardSourceKind.SingleUser, HomePageCardTemplateKind.SingleUser, Symbol.Person, I18NManager.GetResource(HomePageResources.SingleUserCardTitle), I18NManager.GetResource(HomePageResources.SingleUserCardDescription))
    ];
}
