// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Mako.Global.Enum;
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
