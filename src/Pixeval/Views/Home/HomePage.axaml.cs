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
using Pixeval.Models.Options;

namespace Pixeval.Views.Home;

public partial class HomePage : ContentPage
{
    public const decimal MinimumGridSize = 1;
    public const decimal MaximumGridSize = 12;

    private readonly ObservableCollection<HomePageCardLayout> _cards;
    private readonly IReadOnlyList<HomeCardTemplate> _cardTemplates;
    private HomeCardTemplate? _pendingTemplate;
    private bool _isRefreshingGrid;
    private bool _isAddingConfiguredCard;
    private bool _isDisposed;
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
        new(HomePageCardSourceKind.WorkRecommended, HomePageCardTemplateKind.WorkList, Symbol.Calendar),
        new(HomePageCardSourceKind.WorkBookmarks, HomePageCardTemplateKind.WorkList, Symbol.Library),
        new(HomePageCardSourceKind.WorkRanking, HomePageCardTemplateKind.WorkList, Symbol.ArrowTrendingLines),
        new(HomePageCardSourceKind.WorkNew, HomePageCardTemplateKind.WorkList, Symbol.ArrowSync),
        new(HomePageCardSourceKind.WorkFollowing, HomePageCardTemplateKind.WorkList, Symbol.PersonHeart),
        new(HomePageCardSourceKind.WorkPosts, HomePageCardTemplateKind.WorkList, Symbol.AlertUrgent),
        new(HomePageCardSourceKind.WorkSearch, HomePageCardTemplateKind.WorkList, Symbol.SearchSparkle),
        new(HomePageCardSourceKind.UserRecommended, HomePageCardTemplateKind.UserList, Symbol.PeopleCommunity),
        new(HomePageCardSourceKind.UserSearch, HomePageCardTemplateKind.UserList, Symbol.SearchSparkle),
        new(HomePageCardSourceKind.UserFollowing, HomePageCardTemplateKind.UserList, Symbol.PersonHeart),
        new(HomePageCardSourceKind.UserMyPixiv, HomePageCardTemplateKind.UserList, Symbol.People),
        new(HomePageCardSourceKind.Spotlight, HomePageCardTemplateKind.SpotlightList, Symbol.SlideTextSparkle),
        new(HomePageCardSourceKind.SingleImage, HomePageCardTemplateKind.SingleImage, Symbol.Image),
        new(HomePageCardSourceKind.SingleNovel, HomePageCardTemplateKind.SingleNovel, Symbol.BookOpen, WorkType.Novel, SimpleWorkType.Novel),
        new(HomePageCardSourceKind.SingleUser, HomePageCardTemplateKind.SingleUser, Symbol.Person)
    ];
}
