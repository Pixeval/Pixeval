// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Windows.AppLifecycle;
using Pixeval.Activation;
using Pixeval.Database.Managers;
using Pixeval.Messages;
using Pixeval.Util;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Pixeval.AppManagement;
using Pixeval.Attributes;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Logging;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Pages.IllustratorViewer;
using Pixeval.Pages.NovelViewer;
using WinUI3Utilities;

namespace Pixeval.Pages;

public sealed partial class MainPage
{
    public static MainPage Current { get; private set; } = null!;

    private readonly MainPageViewModel _viewModel;

    public FrameworkElement TabViewParameter => MainPageRootTab.TabView;

    public MainPage()
    {
        Current = this;
        _viewModel = new MainPageViewModel(this);
        InitializeComponent();
    }

    private async void CustomizeTitleBar()
    {
        if (Microsoft.UI.Windowing.AppWindowTitleBar.IsCustomizationSupported())
        {
            Window.SetTitleBar(TitleBar);
            return;
        }

        await Task.Yield();
        var logger = App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>();
        logger.LogInformation("Customize title bar is not supported", null);
    }

    public async void MainPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        CustomizeTitleBar();
        App.AppViewModel.AppLoggedIn();

        // The application is invoked by a protocol, call the corresponding protocol handler.
        if (App.AppViewModel.ConsumeProtocolActivation())
        {
            ActivationRegistrar.Dispatch(AppInstance.GetCurrent().GetActivatedEventArgs());
        }

        _ = WeakReferenceMessenger.Default.TryRegister<MainPage, WorkTagClickedMessage>(this, (_, message) =>
        {
            MainPageAutoSuggestionBox.Text = message.Tag;
            Window.AppWindow.MoveInZOrderAtTop();
            PerformSearchWork(message.Type, message.Tag);
        });

        if (_viewModel.MenuItems[(int)App.AppViewModel.AppSettings.DefaultSelectedTabItem] is NavigationViewTag tag)
            MainPageRootTab.AddPage(tag);

        LoadRestrictedModeSettings();
        LoadAiShowSettings();

        using var client = new HttpClient();
        await AppInfo.AppVersion.GitHubCheckForUpdateAsync(client);
        if (AppInfo.AppVersion.UpdateAvailable)
            _viewModel.SettingsTag.ShowIconBadge = true;
        return;

        async void LoadRestrictedModeSettings()
        {
            _viewModel.RestrictedModeProcessing = true;
            try
            {
                RestrictedModeItem.IsChecked = _restrictedCache = await App.AppViewModel.MakoClient.GetRestrictedModeSettingsAsync();
            }
            finally
            {
                _viewModel.RestrictedModeProcessing = false;
            }
        }

        async void LoadAiShowSettings()
        {
            _viewModel.AiShowProcessing = true;
            try
            {
                AiShowItem.IsChecked = _aiShowCache = await App.AppViewModel.MakoClient.GetAiShowSettingsAsync();
            }
            finally
            {
                _viewModel.AiShowProcessing = false;
            }
        }
    }

    private async void NavigationView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs e)
    {
        await Task.Yield();
        sender.SelectedItem = null;

        // The App.AppViewModel.IllustrationDownloadManager will be initialized after that of MainPage object
        // so, we cannot put a navigation tag inside MainPage and treat it as a field, since it will be initialized immediately after
        // the creation of the object while the App.AppViewModel.IllustrationDownloadManager is still null which
        // will lead the program into NullReferenceException on the access of QueuedTasks.

        // args.SelectedItem may be null here
        if (e.InvokedItem is NavigationViewTag tag)
        {
            if (Equals(tag, _viewModel.FeedTag) && App.AppViewModel.AppSettings.WebCookie is "")
                _ = this.CreateAcknowledgementAsync(MainPageResources.FeedTabCannotBeOpenedTitle, MainPageResources.FeedTabCannotBeOpenedContent);
            else
                MainPageRootTab.AddPage(tag);
        }
    }

    private async void KeywordAutoSuggestBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (FocusManager.GetFocusedElement(XamlRoot) is not TextBox)
            return;
        var suggestBox = (AutoSuggestBox)sender;
        suggestBox.IsSuggestionListOpen = true;
        await _viewModel.SuggestionProvider.UpdateAsync(suggestBox.Text);
    }

    /// <summary>
    /// 搜索并跳转至搜索结果
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void KeywordAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion is SuggestionModel
            {
                SuggestionType:
                SuggestionType.Settings or
                SuggestionType.IllustId or
                SuggestionType.NovelId or
                SuggestionType.UserId or
                SuggestionType.UserSearch or
                SuggestionType.IllustrationAutoCompleteTagHeader or
                SuggestionType.IllustrationTrendingTagHeader or
                SuggestionType.NovelTrendingTagHeader or
                SuggestionType.SettingEntryHeader
            })
            return;

        if (args.QueryText.IsNullOrBlank())
        {
            _ = this.CreateAcknowledgementAsync(MainPageResources.SearchKeywordCannotBeBlankTitle,
                MainPageResources.SearchKeywordCannotBeBlankContent);
            return;
        }

        switch (args.ChosenSuggestion)
        {
            case SuggestionModel({ } name, var translatedName, SuggestionType.IllustrationTag):
                PerformSearchWork(SimpleWorkType.IllustAndManga, name, translatedName);
                break;
            case SuggestionModel({ } name, var translatedName, SuggestionType.NovelTag):
                PerformSearchWork(SimpleWorkType.Novel, name, translatedName);
                break;
            case SuggestionModel({ } name, var translatedName, SuggestionType.Tag):
                PerformSearchWork(App.AppViewModel.AppSettings.SimpleWorkType, name, translatedName);
                break;
            default:
                PerformSearchWork(App.AppViewModel.AppSettings.SimpleWorkType, args.QueryText);
                break;
        }
    }

    private async void KeywordAutoSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs e)
    {
        if (e.SelectedItem is SuggestionModel { Name: { Length: > 0 } name, SuggestionType: var type })
        {
            switch (type)
            {
                case SuggestionType.IllustId:
                    if (long.TryParse(sender.Text, out var illustId))
                        await TabViewParameter.CreateIllustrationPageAsync(illustId);
                    break;
                case SuggestionType.NovelId:
                    if (long.TryParse(sender.Text, out var novelId))
                        await TabViewParameter.CreateNovelPageAsync(novelId);
                    break;
                case SuggestionType.UserId:
                    if (long.TryParse(sender.Text, out var userId))
                        await TabViewParameter.CreateIllustratorPageAsync(userId);
                    break;
                case SuggestionType.UserSearch:
                    PerformSearchUser(sender.Text);
                    break;
                case SuggestionType.Settings:
                    if (SettingsEntryAttribute.LazyValues.Value.FirstOrDefault(se => se.LocalizedResourceHeader == name) is { } entry)
                        NavigateToSettingEntry(entry);
                    break;
                default:
                    sender.Text = name;
                    break;
            }
        }
    }

    private async void KeywordAutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        await _viewModel.SuggestionProvider.UpdateAsync(sender.Text);
    }

    private void PerformSearchWork(SimpleWorkType type, string text, string? translatedName = null)
    {
        SearchHistoryPersistentManager.AddHistory(text, translatedName);
        _viewModel.SearchWorksTag.Parameter = (type, text);
        MainPageRootTab.AddPage(_viewModel.SearchWorksTag);
    }

    private void PerformSearchUser(string text)
    {
        SearchHistoryPersistentManager.AddHistory(text);
        _viewModel.SearchUsersTag.Parameter = text;
        MainPageRootTab.AddPage(_viewModel.SearchUsersTag);
    }

    private void OpenSearchSettingButton_OnClicked(object sender, RoutedEventArgs e)
    {
        NavigateToSettingEntry(ReverseSearchApiKeyAttribute.Value);
    }

    private void NavigateToSettingEntry(SettingsEntryAttribute entry) => MainPageRootTab.AddPage(MainPageViewModel.GetSettingsTag(entry));

    /// <summary>
    /// The AutoSuggestBox does not have a 'Paste' event, so we check the keyboard event accordingly
    /// </summary>
    private async void KeywordAutoSuggestionBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.LeftControl).HasFlag(CoreVirtualKeyStates.Down) && e.Key is VirtualKey.V)
        {
            var content = Clipboard.GetContent();
            if (content.AvailableFormats.Contains(StandardDataFormats.StorageItems) &&
                (await content.GetStorageItemsAsync()).FirstOrDefault(i => i.IsOfType(StorageItemTypes.File)) is StorageFile file)
            {
                e.Handled = true; // prevent the event from bubbling if it contains an image, since it means that we want to do reverse search.
                await using var stream = await file.OpenStreamForReadAsync();
                if (string.IsNullOrWhiteSpace(App.AppViewModel.AppSettings.ReverseSearchApiKey))
                    await ShowReverseSearchApiKeyNotPresentDialog();
                else
                    await _viewModel.ReverseSearchAsync(stream);
            }
        }
    }

    private async void ReverseSearchButton_OnClicked(object sender, RoutedEventArgs e)
    {
        if (App.AppViewModel.AppSettings.ReverseSearchApiKey is { Length: > 0 })
        {
            if (await this.OpenFileOpenPickerAsync() is { } file)
            {
                await using var stream = await file.OpenStreamForReadAsync();
                await _viewModel.ReverseSearchAsync(stream);
            }
        }
        else
        {
            await ShowReverseSearchApiKeyNotPresentDialog();
        }
    }

    private Lazy<SettingsEntryAttribute> ReverseSearchApiKeyAttribute { get; } = new(() =>
        SettingsEntryAttribute.GetFromPropertyName(nameof(AppSettings.ReverseSearchApiKey)));

    private async Task ShowReverseSearchApiKeyNotPresentDialog()
    {
        var result = await this.CreateOkCancelAsync(MainPageResources.ReverseSearchApiKeyNotPresentTitle, ReverseSearchApiKeyNotPresentDialogResources.MessageTextBlockText, ReverseSearchApiKeyNotPresentDialogResources.SetApiKeyHyperlinkButtonContent);
        if (result is ContentDialogResult.Primary) 
            NavigateToSettingEntry(ReverseSearchApiKeyAttribute.Value);
    }

    private void KeywordAutoSuggestBox_OnDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private async void KeywordAutoSuggestBox_OnDrop(object sender, DragEventArgs e)
    {
        if (App.AppViewModel.AppSettings.ReverseSearchApiKey is { Length: > 0 })
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems) && (await e.DataView.GetStorageItemsAsync()).ToArray() is [StorageFile item, ..])
            {
                await _viewModel.ReverseSearchAsync(await item.OpenStreamForReadAsync());
            }
        }
        else
        {
            await ShowReverseSearchApiKeyNotPresentDialog();
        }
    }

    private void TitleBar_OnPaneButtonClicked(object? sender, RoutedEventArgs e)
    {
        NavigationView.IsPaneOpen = !NavigationView.IsPaneOpen;
    }

    private bool _restrictedCache;

    private bool _aiShowCache;

    private async void RestrictedModeItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel.RestrictedModeProcessing)
            return;
        var toggleItem = sender.To<ToggleMenuFlyoutItem>();
        _viewModel.RestrictedModeProcessing = true;
        try
        {
            toggleItem.IsChecked = _restrictedCache;
            _ = await App.AppViewModel.MakoClient.PostRestrictedModeSettingsAsync(!_restrictedCache);
            toggleItem.IsChecked = _restrictedCache = await App.AppViewModel.MakoClient.GetRestrictedModeSettingsAsync();
        }
        finally
        {
            _viewModel.RestrictedModeProcessing = false;
        }
    }

    private async void AiShowButtonItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel.AiShowProcessing)
            return;
        var toggleItem = sender.To<ToggleMenuFlyoutItem>();
        _viewModel.AiShowProcessing = true;
        try
        {
            toggleItem.IsChecked = _aiShowCache;
            _ = await App.AppViewModel.MakoClient.PostAiShowSettingsAsync(!_aiShowCache);
            toggleItem.IsChecked = _aiShowCache = await App.AppViewModel.MakoClient.GetAiShowSettingsAsync();
        }
        finally
        {
            _viewModel.AiShowProcessing = false;
        }
    }

    private async void OpenLinkViaTagUri_OnClicked(object sender, RoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(sender.To<FrameworkElement>().GetTag<Uri>());
    }

    private async void OpenLinkViaTag_OnClicked(object sender, RoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(new Uri(sender.To<FrameworkElement>().GetTag<string>()));
    }

    private async void OpenMyPage_OnClick(object sender, RoutedEventArgs e)
    {
        await TabViewParameter.CreateIllustratorPageAsync(_viewModel.Id);
    }

    private async void Logout_OnClicked(object sender, RoutedEventArgs e)
    {
        if (await this.CreateOkCancelAsync(ExitDialogResources.SignOutConfirmationDialogTitle,
                ExitDialogResources.SignOutConfirmationDialogContent) is ContentDialogResult.Primary)
        {
            App.AppViewModel.LoginContext.LogoutExit = true;
            WindowFactory.RootWindow.Close();
        }
    }
}
