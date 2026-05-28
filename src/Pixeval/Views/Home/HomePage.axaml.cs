// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Controls;
using Pixeval.Models.Home;
using Pixeval.ViewModels.Home;

namespace Pixeval.Views.Home;

public partial class HomePage : DrawerPage
{
    public const decimal MinimumGridSize = 1;
    public const decimal MaximumGridSize = 12;

    private readonly ObservableCollection<HomePageCardLayout> _cards;
    private bool _isRefreshingGrid;
    private bool _isDisposed;
    private bool _isUpdatingGridSizeControls;
    private bool _isUpdatingSelectedCardControls;
    private HomePageCardControl? _activeCardControl;
    private HomePageCardLayout? _selectedCard;

    public HomePageViewModel ViewModel { get; }

    public HomePage()
    {
        DataContext = ViewModel = new HomePageViewModel();
        InitializeComponent();

        _cards = App.AppViewModel.AppSettings.HomePageCards;
        ViewModel.PropertyChanged += ViewModel_OnPropertyChanged;
        UpdateGridSizeControls();

        RefreshGrid();
        ApplyDisplaySettings();
    }

    private int RowCount => ViewModel.RowCount;

    private int ColumnCount => ViewModel.ColumnCount;

    private void ViewModel_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not HomePageViewModel viewModel)
            return;
        switch (e.PropertyName)
        {
            case nameof(HomePageViewModel.IsEditMode):
                if (!viewModel.IsEditMode)
                {
                    _activeCardControl?.CancelEdit();
                    _activeCardControl = null;
                    SelectCard(null);
                }

                RefreshEditModeVisuals();
                break;
            case nameof(HomePageViewModel.IsCardTitleHidden):
                ApplyDisplaySettings();
                break;
            case nameof(HomePageViewModel.GridRowsValue):
            case nameof(HomePageViewModel.GridColumnsValue):
                if (!_isUpdatingGridSizeControls)
                    ApplyGridSizeFromViewModel();
                break;
            case nameof(HomePageViewModel.SelectedColumnValue):
            case nameof(HomePageViewModel.SelectedRowValue):
            case nameof(HomePageViewModel.SelectedWidthValue):
            case nameof(HomePageViewModel.SelectedHeightValue):
                if (!_isUpdatingSelectedCardControls)
                    ApplySelectedCardLayoutFromViewModel();
                break;
            case nameof(HomePageViewModel.SelectedBackgroundColor):
                if (!_isUpdatingSelectedCardControls)
                    ApplySelectedCardBackgroundFromViewModel();
                break;
        }
    }
}
