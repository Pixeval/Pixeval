#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/MainPageViewModel.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Pixeval;
using Pixeval.Models;
using Pixeval.Navigation;
using Pixeval.Pages;
using Pixeval.Storage;

namespace Pixeval.ViewModels;

internal partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty] private ImageSource? _avatar;
    [ObservableProperty] private bool _isSuggestionListOpen;
    [ObservableProperty] private string _suggestBoxText;
    [ObservableProperty] private NavigationViewItem _selectedItem;
    
    private readonly INavigationService<MainPage> _navigationService;
    private readonly Data.IRepository<SearchHistory> _searchHistoryRepository;
    private readonly SessionStorage _sessionStorage;
    private readonly SettingStorage _settingStorage;

    public MainPageViewModel(MainPage mainPage,
        INavigationService<MainPage> navigationService,
        Data.IRepository<SearchHistory> repository,
        SessionStorage sessionStorage,
        SettingStorage settingStorage)
    {
        MainPage = mainPage;
        _navigationService = navigationService;
        _searchHistoryRepository = repository;
        _settingStorage = settingStorage;
        _sessionStorage = sessionStorage;
        MainPage.ViewModel = this;
        MainPage.DataContext = this;
        MainPage.InitializeComponent();
    }

    public MainPage MainPage { get; }


}