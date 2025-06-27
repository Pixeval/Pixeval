// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pixeval.AppManagement;
using Pixeval.Attributes;
using Pixeval.Controls.Windowing;
using Mako.Global.Enum;
using Pixeval.Database.Managers;
using Pixeval.Messages;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Pages.IllustratorViewer;
using Pixeval.Pages.Misc;
using Pixeval.Pages.NovelViewer;
using Pixeval.Util;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Misaki;
using WinUI3Utilities;
using Pixeval.Controls;
using System.Collections.Generic;

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
        _viewModel.SubscribeTokenRefresh();

        _ = WeakReferenceMessenger.Default.TryRegister<MainPage, WorkTagClickedMessage>(this, (_, message) =>
        {
            MainPageAutoSuggestionBox.Text = message.Tag;
            Window.AppWindow.MoveInZOrderAtTop();
            PerformSearchWork(message.Type, message.Tag);
        });

        if (_viewModel.MenuItems[(int) App.AppViewModel.AppSettings.DefaultSelectedTabItem] is NavigationViewTag tag)
            MainPageRootTab.AddPage(tag);

        // LoadRestrictedModeSettings();
        LoadAiShowSettings();

        using var client = new HttpClient();
        await AppInfo.AppVersion.GitHubCheckForUpdateAsync(client);
        if (AppInfo.AppVersion.UpdateAvailable)
            _viewModel.SettingsTag.ShowIconBadge = true;
        _viewModel.TryLoadAvatar();
        return;

#pragma warning disable CS8321 // 已声明本地函数，但从未使用过
        async void LoadRestrictedModeSettings()
#pragma warning restore CS8321 // 已声明本地函数，但从未使用过
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
            if (MainPageRootTab.TabView.TabItems.FirstOrDefault(t =>
                    t is TabViewItem { Tag: NavigationViewTag { NavigateTo: { } type } } && tag.NavigateTo == type) is
                TabViewItem item)
            {
                MainPageRootTab.TabView.SelectedItem = item;
                return;
            }

            if (Equals(tag, _viewModel.FeedTag) && App.AppViewModel.AppSettings.WebCookie is "")
                _ = this.CreateAcknowledgementAsync(MainPageResources.FeedTabCannotBeOpenedTitle, MainPageResources.FeedTabCannotBeOpenedContent);
            else
            {
                if (Equals(tag, _viewModel.ExtensionsTag) && App.AppViewModel.VersionContext.NeverUsedExtensions)
                    // AppInfo.SaveVersionContext(); 在ExtensionsPage中调用
                    _viewModel.ExtensionsTag.ShowIconBadge = false;

                MainPageRootTab.AddPage(tag);
            }
        }
    }

    private async void KeywordAutoSuggestBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (FocusManager.GetFocusedElement(XamlRoot) is not TextBox)
            return;
        var suggestBox = (AutoSuggestBox) sender;
        suggestBox.IsSuggestionListOpen = true;
        await _viewModel.SuggestionProvider.UpdateAsync(suggestBox.Text);
    }

    /// <summary>
    /// 搜索并跳转至搜索结果
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void KeywordAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs e)
    {
        if (e.ChosenSuggestion is not SuggestionModel
            {
                Name: { } name,
                TranslatedName: var translatedName,
                SuggestionType:
                SuggestionType.Tag or
                SuggestionType.IllustrationTag or
                SuggestionType.NovelTag or
                SuggestionType.History
            } model)
            return;

        if (string.IsNullOrWhiteSpace(name))
        {
            _ = this.CreateAcknowledgementAsync(MainPageResources.SearchKeywordCannotBeBlankTitle,
                MainPageResources.SearchKeywordCannotBeBlankContent);
            return;
        }

        switch (model.SuggestionType)
        {
            case SuggestionType.IllustrationTag:
                PerformSearchWork(SimpleWorkType.IllustAndManga, name, translatedName);
                break;
            case SuggestionType.NovelTag:
                PerformSearchWork(SimpleWorkType.Novel, name, translatedName);
                break;
            case SuggestionType.Tag:
                PerformSearchWork(App.AppViewModel.AppSettings.SimpleWorkType, name, translatedName);
                break;
            default:
                PerformSearchWork(App.AppViewModel.AppSettings.SimpleWorkType, name);
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
                        await TabViewParameter.CreateIllustrationPageAsync(illustId.ToString(), IPlatformInfo.Pixiv);
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
                    return;
            }
        }
    }

    private string _lastText = "";

    private async void KeywordAutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e)
    {
        if (e.Reason is AutoSuggestionBoxTextChangeReason.SuggestionChosen)
            sender.Text = _lastText;
        else
        {
            _lastText = sender.Text;
            await _viewModel.SuggestionProvider.UpdateAsync(_lastText);
        }
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

    private void NavigateToSettingEntry(SettingsEntryAttribute entry)
    {
        if (MainPageRootTab.TabView.TabItems.FirstOrDefault(t => t is TabViewItem { Content: Frame { Content: SettingsPage } }) is
            TabViewItem { Content: Frame { Content: SettingsPage page } } item)
        {
            if (Equals(MainPageRootTab.TabView.SelectedItem, item))
                page.ScrollToAttribute(entry);
            else
            {
                page.TargetAttribute = entry;
                MainPageRootTab.TabView.SelectedItem = item;
            }
        }
        else
            MainPageRootTab.AddPage(MainPageViewModel.GetSettingsTag(entry));
    }

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
                    await ReverseSearchAsync(stream);
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
                await ReverseSearchAsync(stream);
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

    private void TitleBar_OnPaneButtonClicked(TitleBar sender, object e)
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

    private async void MainPage_OnDragEnter(object sender, DragEventArgs e)
    {
        var deferral = e.GetDeferral();
        if (e.DataView.Contains(StandardDataFormats.StorageItems) &&
            await e.DataView.GetStorageItemsAsync() is [StorageFile])
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            deferral.Complete();
            ImageSearchGrid.Opacity = 1;
            CanvasControl.Invalidate();
        }
    }

    private void MainPage_OnDragLeave(object sender, DragEventArgs e)
    {
        if (e.AllowedOperations is not DataPackageOperation.None)
            ImageSearchGrid.Opacity = 0;
    }

    private async void MainPage_OnDrop(object sender, DragEventArgs e)
    {
        ImageSearchGrid.Opacity = 0;
        if (App.AppViewModel.AppSettings.ReverseSearchApiKey is { Length: > 0 })
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems) && await e.DataView.GetStorageItemsAsync() is [StorageFile item])
                await ReverseSearchAsync(await item.OpenStreamForReadAsync());
        }
        else
        {
            await ShowReverseSearchApiKeyNotPresentDialog();
        }
    }

    private void CanvasControl_OnDraw(CanvasControl sender, CanvasDrawEventArgs e)
    {
        const float strokeWidth = 5;
        const float halfStrokeWidth = strokeWidth / 2;
        e.DrawingSession.Clear(Colors.Transparent);
        e.DrawingSession.DrawRoundedRectangle(halfStrokeWidth, halfStrokeWidth, (float) sender.ActualWidth - strokeWidth, (float) sender.ActualHeight - strokeWidth, 8, 8,
            _Color, strokeWidth, new()
            {
                DashCap = CanvasCapStyle.Round,
                LineJoin = CanvasLineJoin.Round,
                DashStyle = CanvasDashStyle.Dash
            });
    }

    private static readonly Color _Color = Application.Current.GetResource<SolidColorBrush>("TextFillColorPrimaryBrush").Color;

    private async Task ReverseSearchAsync(Stream stream)
    {
        try
        {
            this.InfoGrowl(MainPageResources.ReverseSearchStartContent);
            var result = await App.AppViewModel.MakoClient.ReverseSearchAsync(stream, App.AppViewModel.AppSettings.ReverseSearchApiKey);
            var models = new List<IArtworkInfo>();
            foreach (var r in result)
            {
                if (r.Header.Similarity < App.AppViewModel.AppSettings.ReverseSearchResultSimilarityThreshold)
                    continue;
                if (r.Data.PixivId is { } pixivId)
                    models.AddIfNotNull(await pixivId.ToString().TryGetIArtworkInfoAsync(IPlatformInfo.Pixiv));

                if (r.Data.DanbooruId is { } danbooruId)
                    models.AddIfNotNull(await danbooruId.ToString().TryGetIArtworkInfoAsync(IPlatformInfo.Danbooru));

                if (r.Data.YandereId is { } yandereId)
                    models.AddIfNotNull(await yandereId.ToString().TryGetIArtworkInfoAsync(IPlatformInfo.Yandere));

                if (r.Data.GelbooruId is { } gelbooruId)
                    models.AddIfNotNull(await gelbooruId.ToString().TryGetIArtworkInfoAsync(IPlatformInfo.Gelbooru));

                if (r.Data.SankakuId is { } sankakuId)
                    models.AddIfNotNull(await sankakuId.ToString().TryGetIArtworkInfoAsync(IPlatformInfo.Sankaku));
            }

            if (models.Count is 0)
                this.ErrorGrowl(MainPageResources.ReverseSearchNotFoundTitle,
                    MainPageResources.ReverseSearchNotFoundContent);
            else
            {
                var viewModels = models.Select(IllustrationItemViewModel.CreateInstance).ToArray();
                MainPageRootTab.CreateIllustrationPage(viewModels[0], viewModels);
            }
            /*
                this.ErrorGrowl(MainPageResources.ReverseSearchErrorTitle,
                    result.Header.Status > 0
                        ? MainPageResources.ReverseSearchServerSideErrorContent
                        : MainPageResources.ReverseSearchClientSideErrorContent);
            */
        }
        catch (Exception e)
        {
            this.ErrorGrowl(MiscResources.ExceptionEncountered, e.ToString());
        }
    }
}
