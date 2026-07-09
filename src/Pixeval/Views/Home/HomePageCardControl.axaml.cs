// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using Mako.Engine.Implements;
using Mako.Model;
using Pixeval.Models.Home;
using Pixeval.Models.Options;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.ViewModels.Home;
using Pixeval.Views.Capability;
using Pixeval.Views.Search;
using Pixeval.Views.Viewers;

namespace Pixeval.Views.Home;

public sealed partial class HomePageCardControl : TemplatedControl, IDisposable
{
    private const string PcEditing = ":editing";
    private const string PcSelected = ":selected";

    public static readonly DirectProperty<HomePageCardControl, HomePageCardLayout> CardProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, HomePageCardLayout>(nameof(Card), control => control.Card, (control, value) => control.Card = value);

    public static readonly DirectProperty<HomePageCardControl, HomeCardTemplate> CardTemplateProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, HomeCardTemplate>(nameof(CardTemplate), control => control.CardTemplate,
            (control, value) => control.CardTemplate = value);

    public static readonly DirectProperty<HomePageCardControl, int> RowCountProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, int>(nameof(RowCount), control => control.RowCount, (control, value) => control.RowCount = value);

    public static readonly DirectProperty<HomePageCardControl, int> ColumnCountProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, int>(nameof(ColumnCount), control => control.ColumnCount, (control, value) => control.ColumnCount = value);

    public static readonly StyledProperty<bool> IsEditingProperty =
        AvaloniaProperty.Register<HomePageCardControl, bool>(nameof(IsEditing));

    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<HomePageCardControl, bool>(nameof(IsSelected));

    public static readonly StyledProperty<bool> IsCardTitleVisibleProperty =
        AvaloniaProperty.Register<HomePageCardControl, bool>(nameof(IsCardTitleVisible), true);

    public static readonly DirectProperty<HomePageCardControl, string> CardTitleProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, string>(nameof(CardTitle), control => control.CardTitle);

    public static readonly DirectProperty<HomePageCardControl, Symbol> CardSymbolProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, Symbol>(nameof(CardSymbol), control => control.CardSymbol);

    public static readonly DirectProperty<HomePageCardControl, HomeCardPreviewViewModel?> PreviewViewModelProperty =
        AvaloniaProperty.RegisterDirect<HomePageCardControl, HomeCardPreviewViewModel?>(nameof(PreviewViewModel), control => control.PreviewViewModel,
            (control, value) => control.PreviewViewModel = value);

    private static readonly HomeCardTemplate _PlaceholderTemplate = new(
        HomePageCardSourceKind.WorkRecommended);

    private Panel? _rootGrid;
    private Panel? _resizeHandlesLayer;
    private Button? _quickDeleteButton;
    private PointerEditState? _pointerEditState;
    private bool _isDisposed;

    public HomePageCardControl(
        HomePageCardLayout card,
        HomeCardTemplate template,
        int rowCount,
        int columnCount)
    {
        Card = card;
        CardTemplate = template;
        RowCount = rowCount;
        ColumnCount = columnCount;
        PreviewViewModel = new(card);
        Loaded += HomePageCardControl_OnLoaded;
    }

    public HomePageCardControl()
    {
        PreviewViewModel = new(Card);
    }

    public event EventHandler<HomeCardSelectedEventArgs>? CardSelected;

    public event EventHandler<HomeCardEditPreviewEventArgs>? EditPreview;

    public event EventHandler<HomeCardEditCompletedEventArgs>? EditCompleted;

    public event EventHandler<HomeCardDeleteRequestedEventArgs>? DeleteRequested;

    public HomePageCardLayout Card
    {
        get;
        private set
        {
            var oldTitle = CardTitle;
            SetAndRaise(CardProperty, ref field, value);
            RaisePropertyChanged(CardTitleProperty, oldTitle, CardTitle);
        }
    } = new();

    public HomeCardTemplate CardTemplate
    {
        get;
        private set
        {
            var oldTitle = CardTitle;
            var oldSymbol = CardSymbol;
            SetAndRaise(CardTemplateProperty, ref field, value);
            RaisePropertyChanged(CardTitleProperty, oldTitle, CardTitle);
            RaisePropertyChanged(CardSymbolProperty, oldSymbol, CardSymbol);
            Background = new SolidColorBrush(Color.FromUInt32(Card.BackgroundColor));
        }
    } = _PlaceholderTemplate;

    public int RowCount
    {
        get;
        private set => SetAndRaise(RowCountProperty, ref field, value);
    } = 1;

    public int ColumnCount
    {
        get;
        private set => SetAndRaise(ColumnCountProperty, ref field, value);
    } = 1;

    public string CardTitle => Card.ToString();

    public Symbol CardSymbol => CardTemplate.Symbol;

    public bool IsEditing
    {
        get => GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public bool IsCardTitleVisible
    {
        get => GetValue(IsCardTitleVisibleProperty);
        set => SetValue(IsCardTitleVisibleProperty, value);
    }

    public HomeCardPreviewViewModel? PreviewViewModel
    {
        get;
        private set
        {
            if (ReferenceEquals(field, value))
                return;

            var old = field;
            SetAndRaise(PreviewViewModelProperty, ref field, value);
            old?.Dispose();
        }
    }

    public void CancelEdit() => CompleteEdit();

    public void UpdateGridSize(int rowCount, int columnCount)
    {
        RowCount = rowCount;
        ColumnCount = columnCount;
    }

    public void UpdateBackground()
    {
        Background = new SolidColorBrush(Color.FromUInt32(Card.BackgroundColor));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachTemplateHandlers();

        base.OnApplyTemplate(e);

        _rootGrid = e.NameScope.Find<Panel>(PartRootGrid);
        _resizeHandlesLayer = e.NameScope.Find<Panel>(PartResizeHandlesLayer);
        _quickDeleteButton = e.NameScope.Find<Button>(PartQuickDeleteButton);

        if (_rootGrid is not null)
        {
            _rootGrid.PointerCaptureLost += Card_OnPointerCaptureLost;
            _rootGrid.PointerMoved += Card_OnPointerMoved;
            _rootGrid.PointerPressed += Card_OnPointerPressed;
            _rootGrid.PointerReleased += Card_OnPointerReleased;
        }

        _resizeHandlesLayer?.PointerPressed += ResizeHandle_OnPointerPressed;
        _quickDeleteButton?.Click += QuickDeleteButton_OnClick;
    }

    private async void HomePageCardControl_OnLoaded(object? sender, RoutedEventArgs e)
    {
        Loaded -= HomePageCardControl_OnLoaded;
        if (PreviewViewModel is not null)
            await PreviewViewModel.LoadAsync();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsEditingProperty)
        {
            PseudoClasses.Set(PcEditing, change.GetNewValue<bool>());

            if (change.GetOldValue<bool>() && !change.GetNewValue<bool>())
                CompleteEdit();

            return;
        }

        if (change.Property == IsSelectedProperty)
            PseudoClasses.Set(PcSelected, change.GetNewValue<bool>());
    }

    private void DetachTemplateHandlers()
    {
        _rootGrid?.PointerCaptureLost -= Card_OnPointerCaptureLost;
        _rootGrid?.PointerMoved -= Card_OnPointerMoved;
        _rootGrid?.PointerPressed -= Card_OnPointerPressed;
        _rootGrid?.PointerReleased -= Card_OnPointerReleased;

        _resizeHandlesLayer?.PointerPressed -= ResizeHandle_OnPointerPressed;
        _quickDeleteButton?.Click -= QuickDeleteButton_OnClick;

        _rootGrid = null;
        _resizeHandlesLayer = null;
        _quickDeleteButton = null;
    }

    private void QuickDeleteButton_OnClick(object? sender, RoutedEventArgs e)
    {
        DeleteRequested?.Invoke(this, new(Card));
        e.Handled = true;
    }

    [RelayCommand]
    private async Task OpenPreviewItemAsync(object? parameter)
    {
        if (TopLevel.GetTopLevel(this) is not { } topLevel)
            return;

        switch (parameter)
        {
            case NovelItemViewModel viewModel:
                OpenNovel(topLevel, viewModel);
                break;
            case IllustrationItemViewModel viewModel:
                OpenIllustration(topLevel, viewModel);
                break;
            case UserItemViewModel viewModel:
                if (topLevel.ViewContainer is { } viewContainer)
                    viewContainer.CreateUserPage(viewModel.UserId);
                break;
            case SpotlightItemViewModel viewModel:
                if (topLevel.Launcher is { } launcher)
                    await launcher.LaunchUriAsync(new(viewModel.Entry.ArticleUrl));
                break;
        }
    }

    [RelayCommand]
    private async Task OpenCardPageAsync()
    {
        if (PreviewViewModel is null)
            return;

        await PreviewViewModel.EnsureLoadedAsync();
        if (TopLevel.GetTopLevel(this) is not { ViewContainer: { } viewContainer })
            return;

        switch (Card.SourceKind)
        {
            case HomePageCardSourceKind.WorkRecommended:
                viewContainer.NavigateTo(new WorkRecommendedPage(Card.WorkType, CloneWorkViewModel()));
                break;
            case HomePageCardSourceKind.WorkNew:
                viewContainer.NavigateTo(new WorkNewPage(Card.WorkType, CloneWorkViewModel()));
                break;
            case HomePageCardSourceKind.WorkPosts:
                viewContainer.NavigateTo(new WorkPostsPage(CreateUserBasicInfo(Card), Card.WorkType, CloneWorkViewModel()));
                break;
            case HomePageCardSourceKind.WorkBookmarks:
                viewContainer.NavigateTo(new WorkBookmarksPage(
                    CreateUserBasicInfo(Card),
                    Card.SimpleWorkType,
                    Card.PrivacyPolicy,
                    Card.Tag,
                    CloneWorkViewModel()));
                break;
            case HomePageCardSourceKind.WorkRanking:
                viewContainer.NavigateTo(new WorkRankingPage(
                    Card.SimpleWorkType,
                    Card.RankOption,
                    Card.GetRankingDate().LocalDateTime,
                    CloneWorkViewModel()));
                break;
            case HomePageCardSourceKind.WorkFollowing:
                viewContainer.NavigateTo(new WorkFollowingPage(Card.SimpleWorkType, Card.PrivacyPolicy, CloneWorkViewModel()));
                break;
            case HomePageCardSourceKind.WorkMyPixiv:
                viewContainer.NavigateTo(new WorkMyPixivPage(Card.SimpleWorkType, CloneWorkViewModel()));
                break;
            case HomePageCardSourceKind.WorkRelated:
                viewContainer.NavigateTo(new WorkRelatedPage(Card.EntryId, Card.SimpleWorkType, CloneWorkViewModel()));
                break;
            case HomePageCardSourceKind.WorkSearch:
                var searchText = Card.SearchText ?? "";
                viewContainer.NavigateTo(new WorkSearchResultPage(
                    searchText,
                    new IllustrationSearchArguments(searchText),
                    new NovelSearchArguments(searchText),
                    Card.SimpleWorkType,
                    CloneWorkViewModel()));
                break;
            case HomePageCardSourceKind.UserRecommended:
                viewContainer.NavigateTo(new UserRecommendPage(CloneUserViewModel()));
                break;
            case HomePageCardSourceKind.UserSearch:
                viewContainer.NavigateTo(new UserSearchPage(Card.SearchText, CloneUserViewModel()));
                break;
            case HomePageCardSourceKind.UserFollowing:
                viewContainer.NavigateTo(new UserFollowingPage(Card.UserId, Card.PrivacyPolicy, CloneUserViewModel()));
                break;
            case HomePageCardSourceKind.UserFollower:
                viewContainer.NavigateTo(new UserFollowerPage(CloneUserViewModel()));
                break;
            case HomePageCardSourceKind.UserMyPixiv:
                viewContainer.NavigateTo(new UserMyPixivPage(Card.UserId, CloneUserViewModel()));
                break;
            case HomePageCardSourceKind.Spotlight:
                viewContainer.NavigateTo(new SpotlightPage(CloneSpotlightViewModel()));
                break;
            case HomePageCardSourceKind.SingleImage:
            case HomePageCardSourceKind.SingleNovel:
            case HomePageCardSourceKind.SingleUser:
                await OpenFirstPreviewItemAsync();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(Card.SourceKind));
        }
    }

    private Task OpenFirstPreviewItemAsync() =>
        PreviewViewModel?.Items.FirstOrDefault() is { } item
            ? OpenPreviewItemAsync(item)
            : Task.CompletedTask;

    private IWorkViewViewModel? CloneWorkViewModel() =>
        PreviewViewModel?.ViewModel switch
        {
            IllustrationViewViewModel viewModel => new IllustrationViewViewModel(viewModel),
            NovelViewViewModel viewModel => new NovelViewViewModel(viewModel),
            _ => null
        };

    private UserViewViewModel? CloneUserViewModel() =>
        PreviewViewModel?.ViewModel is UserViewViewModel viewModel ? new(viewModel) : null;

    private SpotlightViewViewModel? CloneSpotlightViewModel() =>
        PreviewViewModel?.ViewModel is SpotlightViewViewModel viewModel ? new(viewModel) : null;

    private static UserBasicInfo CreateUserBasicInfo(HomePageCardLayout card) =>
        PixevalSettings.Me is { Id: var meId } me && card.UserId == meId
            ? me
            : new HomeCardUserBasicInfo(card.UserId, card.ToString());

    private void OpenNovel(TopLevel topLevel, NovelItemViewModel viewModel)
    {
        if (topLevel.ViewContainer is not { } viewContainer)
            return;

        if (PreviewViewModel?.ViewModel is NovelViewViewModel viewViewModel)
            viewContainer.CreateNovelPage(viewModel, viewViewModel.DataProvider.CloneRef());
        else
            viewContainer.CreateNovelPage(viewModel.Entry.Id);
    }

    private void OpenIllustration(TopLevel topLevel, IllustrationItemViewModel viewModel)
    {
        if (topLevel.ViewContainer is not { } viewContainer)
            return;

        if (PreviewViewModel?.ViewModel is IllustrationViewViewModel viewViewModel)
        {
            viewContainer.CreateIllustrationPage(viewModel, viewViewModel.DataProvider.CloneRef());
            return;
        }

        if (viewModel.Entry is Illustration illustration)
        {
            viewContainer.CreateIllustrationPage(illustration);
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        Loaded -= HomePageCardControl_OnLoaded;
        CompleteEdit(false);
        DetachTemplateHandlers();
        CardSelected = null;
        EditPreview = null;
        EditCompleted = null;
        DeleteRequested = null;
        PreviewViewModel = null;
    }
}
