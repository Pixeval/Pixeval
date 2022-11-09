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
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Net;
using Pixeval.Data;
using Pixeval.Messages;
using Pixeval.Models;
using Pixeval.Pages;
using Pixeval.Startup.WinUI.Navigation;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Microsoft.UI.Xaml;
using Image = SixLabors.ImageSharp.Image;
using Pixeval.Storage;

namespace Pixeval.ViewModels;

internal partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty] private ImageSource? _avatar;
    [ObservableProperty] private bool _isSuggestionListOpen;
    [ObservableProperty] private string _suggestBoxText;
    [ObservableProperty] private NavigationViewItem _selectedItem;

    private readonly MainPage _mainPage;
    private readonly INavigationService<MainPage> _navigationService;
    private readonly IBaseRepository<SearchHistory> _searchHistoryRepository;
    private readonly SessionStorage _sessionStorage;
    private readonly SettingStorage _settingStorage;

    public MainPageViewModel(MainPage mainPage,
        INavigationService<MainPage> navigationService,
        IBaseRepository<SearchHistory> searchHistoryRepository,
        SessionStorage sessionStorage,
        SettingStorage settingStorage)
    {
        _mainPage = mainPage;
        _navigationService = navigationService;
        _searchHistoryRepository = searchHistoryRepository;
        _settingStorage = settingStorage;
        _sessionStorage = sessionStorage;
        _mainPage.ViewModel = this;
        _mainPage.DataContext = this;
        _mainPage.InitializeComponent();
    }
    

    public double MainPageRootNavigationViewOpenPanelLength => 280;


    [RelayCommand]
    private async Task AutoSuggestBoxGotFocusAsync()
    {
        IsSuggestionListOpen = true;
    }

    [RelayCommand]
    private async Task AutoSuggestBoxQuerySubmittedAsync(AutoSuggestBoxQuerySubmittedEventArgs args)
    {

    }

    [RelayCommand]
    private void AutoSuggestBoxSuggestionChosen(AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        //if (args.SelectedItem is SuggestionModel { Name: { Length: > 0 } name })
        //{
        //    SuggestBoxText = name;
        //}
    }

    [RelayCommand]
    private async Task AutoSuggestBoxTextChangedAsync(AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        //await SuggestionProvider.UpdateAsync(SuggestBoxText);
    }

    [RelayCommand]
    private async Task NavigationViewSelectionChangedAsync(NavigationViewItem downloadListTab)
    {
        if (Equals(SelectedItem, _mainPage.DownloadsTab))
        {
            await _navigationService.NavigateToAsync("/Downloads");
            return;
        }

        await _navigationService.NavigateToAsync((string)SelectedItem.Tag, new SuppressNavigationTransitionInfo());
    }

    [RelayCommand]
    private async Task OpenSearchSettingAsync()
    {
        await _navigationService.NavigateToAsync("/Settings");
        // The stupid delay here does merely nothing but wait the navigation to complete, apparently
        // the navigation is asynchronous and there's no way to wait for it
        //await Task.Delay(500);
        //var settingsPage = MainPageFrame.FindDescendant<SettingsPage>()!;
        //var position = settingsPage.SearchSettingsGroup
        //    .TransformToVisual((UIElement)settingsPage.SettingsPageScrollViewer.Content)
        //    .TransformPoint(new Point(0, 0));
        //settingsPage.SettingsPageScrollViewer.ChangeView(null, position.Y, null, false);
    }

    [RelayCommand]
    private async Task AutoSuggestBoxPasteAsync(KeyRoutedEventArgs args)
    {
        if (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.LeftControl).HasFlag(CoreVirtualKeyStates.Down) && args.Key == VirtualKey.V)
        {
            var content = Clipboard.GetContent();
            if (content.AvailableFormats.Contains(StandardDataFormats.StorageItems) &&
                (await content.GetStorageItemsAsync()).FirstOrDefault(i => i.IsOfType(StorageItemTypes.File)) is StorageFile file)
            {
                args.Handled = true; // prevent the event from bubbling if it contains an image, since it means that we want to do reverse search.
                await using var stream = await file.OpenStreamForReadAsync();
                if (await Image.DetectFormatAsync(stream) is not null)
                {
                    if ((await _settingStorage.GetSettingAsync())?.ReverseSearchApiKey is not { Length: > 0 })
                    {
                        await ShowReverseSearchApiKeyNotPresentDialogAsync();
                        return;
                    }

                    await PerformReverseSearchAsync(stream);
                }
            }
        }
    }

    [RelayCommand]
    private async Task ReverseSearchAsync()
    {
        if ((await _settingStorage.GetSettingAsync())?.ReverseSearchApiKey is { Length: > 0 })
        {
            if (await UIHelper.OpenFileOpenPicker(Window.Current) is { } file)
            {
                await using var stream = await file.OpenStreamForReadAsync();
                await PerformReverseSearchAsync(stream);
            }
        }
        else
        {
            await ShowReverseSearchApiKeyNotPresentDialogAsync();
        }
    }

    private async Task ShowReverseSearchApiKeyNotPresentDialogAsync()
    {
        //var content = new ReverseSearchApiKeyNotPresentDialog(_appViewModel);
        //var dialog = MessageDialogBuilder.Create().WithTitle(MainPageResources.ReverseSearchApiKeyNotPresentTitle)
        //    .WithContent(content)
        //    .WithPrimaryButtonText(MessageContentDialogResources.OkButtonContent)
        //    .WithDefaultButton(ContentDialogButton.Primary)
        //    .Build(_appViewModel.Window);
        //content.Owner = dialog;
        //await dialog.ShowAsync();
    }


    private async Task PerformSearchAsync(string text, string? optTranslatedName = null)
    {
        if (await _searchHistoryRepository.CountAsync() == 0 || await _searchHistoryRepository.Collection.FindOneAsync(_ => _.Value != text) is not null)
        {
            await _searchHistoryRepository.CreateAsync(new SearchHistory
            {
                Value = text,
                TranslatedName = optTranslatedName,
                Time = DateTime.Now
            });
        }

        //var setting = _settingsContainer.Setting;
        //await _navigationService.NavigateToAsync("SearchResults", _appViewModel.MakoClient.Search(
        //    text,
        //    setting.SearchStartingFromPageNumber,
        //    setting.PageLimitForKeywordSearch,
        //    setting.TagMatchOption,
        //    setting.DefaultSortOption,
        //    setting.SearchDuration,
        //    setting.TargetFilter,
        //    setting.UsePreciseRangeForSearch ? setting.SearchStartDate : null,
        //    setting.UsePreciseRangeForSearch ? setting.SearchEndDate : null));
    }

    /// <summary>
    ///     Download user's avatar and set to the Avatar property.
    /// </summary>
    public async Task DownloadAndSetAvatarAsync()
    {
        //var makoClient = _appViewModel.MakoClient;
        //// get byte array of avatar
        //// and set to the bitmap image
        //Avatar = await (await makoClient.GetMakoHttpClient(PixivApiKind.ImageApi).DownloadAsIRandomAccessStreamAsync(makoClient.PixivSession.AvatarUrl!))
        //    .GetOrThrow()
        //    .GetBitmapImageAsync(true);
    }


    public async Task PerformReverseSearchAsync(Stream stream)
    {
        //try
        //{
        //    _appViewModel.Window.ShowProgressRing();
        //    var result = await _appViewModel.MakoClient.ReverseSearchAsync(stream, _appConfigurationManager.Setting.ReverseSearchApiKey!);
        //    if (result.Header is not null)
        //    {
        //        switch (result.Header!.Status)
        //        {
        //            case 0:
        //                if (result.Results?.FirstOrDefault() is { Header.IndexId: 5 or 6 } first)
        //                {
        //                    var viewModels = new IllustrationViewModel(await _appViewModel.MakoClient.GetIllustrationFromIdAsync(first.Data!.PixivId.ToString()))
        //                        .GetMangaIllustrationViewModels()
        //                        .ToArray();
        //                    _appViewModel.Window.HideProgressRing();
        //                    ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", _appViewModel.AppWindowRootFrame);
        //                    _appViewModel.RootFrameNavigate(typeof(IllustrationViewerPage), new IllustrationViewerPageViewModel(viewModels), new SuppressNavigationTransitionInfo());
        //                    return;
        //                }

        //                break;
        //            case var s:
        //                await MessageDialogBuilder.CreateAcknowledgement(
        //                        _appViewModel.Window,
        //                        MainPageResources.ReverseSearchErrorTitle,
        //                        s > 0 ? MainPageResources.ReverseSearchServerSideErrorContent : MainPageResources.ReverseSearchClientSideErrorContent)
        //                    .ShowAsync();
        //                break;
        //        }

        //        _appViewModel.Window.HideProgressRing();
        //        MessageDialogBuilder.CreateAcknowledgement(_appViewModel.Window, MainPageResources.ReverseSearchNotFoundTitle, MainPageResources.ReverseSearchNotFoundContent);
        //    }
        //}
        //catch (Exception e)
        //{
        //    _appViewModel.Window.HideProgressRing();
        //    await _appViewModel.ShowExceptionDialogAsync(e);
        //}
    }
}