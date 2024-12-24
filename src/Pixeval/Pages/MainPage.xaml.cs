#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/MainPage.xaml.cs
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

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppLifecycle;
using Pixeval.Activation;
using Pixeval.Database;
using Pixeval.Database.Managers;
using Pixeval.Messages;
using Pixeval.Pages.Capability;
using Pixeval.Pages.Misc;
using Pixeval.Util;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities;
using Pixeval.AppManagement;
using Pixeval.Attributes;
using Pixeval.Controls.Windowing;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.Logging;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Pages.IllustratorViewer;
using Pixeval.Pages.NovelViewer;

namespace Pixeval.Pages;

public sealed partial class MainPage
{
    private readonly MainPageViewModel _viewModel;

    private NavigationViewItem? _lastSelected;

    public MainPage()
    {
        _viewModel = new MainPageViewModel(this);
        InitializeComponent();
        CustomizeTitleBar();
    }

    private static async void CustomizeTitleBar()
    {
        if (AppInfo.CustomizeTitleBarSupported)
            return;

        await Task.Yield();
        var logger = App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>();
        logger.LogInformation("Customize title bar is not supported", null);
    }

    public override async void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        App.AppViewModel.AppLoggedIn();

        // The application is invoked by a protocol, call the corresponding protocol handler.
        if (App.AppViewModel.ConsumeProtocolActivation())
        {
            ActivationRegistrar.Dispatch(AppInstance.GetCurrent().GetActivatedEventArgs());
        }

        _ = WeakReferenceMessenger.Default.TryRegister<MainPage, WorkTagClickedMessage>(this, (_, message) =>
        {
            var window = WindowFactory.ForkedWindows[HWnd];
            window.AppWindow.MoveInZOrderAtTop();
            PerformSearchWork(message.Type, message.Tag);
        });
        using var client = new HttpClient();
        await AppInfo.AppVersion.GitHubCheckForUpdateAsync(client);
        if (AppInfo.AppVersion.UpdateAvailable)
            InfoBadge.Visibility = Visibility.Visible;
    }

    private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        // dirty trick, the order of the menu items is the same as the order of the fields in MainPageTabItem
        // since enums are basically integers, we just need a cast to transform it to the correct offset.
        NavigationView.SelectedItem = NavigationView.MenuItems[(int)App.AppViewModel.AppSettings.DefaultSelectedTabItem];
    }

    private async void NavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        await Task.Yield();

        // The App.AppViewModel.IllustrationDownloadManager will be initialized after that of MainPage object
        // so, we cannot put a navigation tag inside MainPage and treat it as a field, since it will be initialized immediately after
        // the creation of the object while the App.AppViewModel.IllustrationDownloadManager is still null which
        // will lead the program into NullReferenceException on the access of QueuedTasks.

        // args.SelectedItem may be null here
        if (sender.SelectedItem is NavigationViewItem { Tag: NavigationViewTag tag } selectedItem)
        {
            if (Equals(selectedItem, FeedTab) && App.AppViewModel.AppSettings.WebCookie is "")
            {
                _ = this.CreateAcknowledgementAsync(MainPageResources.FeedTabCannotBeOpenedTitle, MainPageResources.FeedTabCannotBeOpenedContent);
                sender.SelectedItem = _lastSelected;
                return;
            }

            if (Equals(selectedItem, DownloadListTab) || Equals(selectedItem, SettingsTab) || Equals(selectedItem, TagsTab))
                Navigate(MainPageRootFrame, tag);
            else
                MainPageRootFrame.NavigateTag(tag, new SuppressNavigationTransitionInfo());
        }

        _lastSelected = sender.SelectedItem as NavigationViewItem;
    }

    private void MainPageRootFrame_OnNavigated(object sender, NavigationEventArgs e)
    {
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect();
        // sender.To<Frame>().BackStack.Clear();
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
                        await IllustrationViewerHelper.CreateWindowWithPageAsync(illustId);
                    break;
                case SuggestionType.NovelId:
                    if (long.TryParse(sender.Text, out var novelId))
                        await NovelViewerHelper.CreateWindowWithPageAsync(novelId);
                    break;
                case SuggestionType.UserId:
                    if (long.TryParse(sender.Text, out var userId))
                        await IllustratorViewerHelper.CreateWindowWithPageAsync(userId);
                    break;
                case SuggestionType.UserSearch:
                    PerformSearchUser(sender.Text);
                    break;
                case SuggestionType.Settings:
                    if (SettingsEntryAttribute.LazyValues.Value.FirstOrDefault(se => se.LocalizedResourceHeader == name) is { } entry)
                        await NavigateToSettingEntryAsync(entry);
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

    private void PerformSearchWork(SimpleWorkType type, string text, string? optTranslatedName = null)
    {
        PerformSearch(text, optTranslatedName);
        _ = MainPageRootFrame.Navigate(typeof(SearchWorksPage), (type, text));
    }

    private void PerformSearchUser(string text)
    {
        PerformSearch(text);
        _ = MainPageRootFrame.Navigate(typeof(SearchUsersPage), text);
    }

    private void PerformSearch(string text, string? optTranslatedName = null)
    {
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<SearchHistoryPersistentManager>();
        if (manager.Count is 0 || manager.Select(count: 1).FirstOrDefault() is { Value: var last } && last != text)
        {
            manager.Insert(new SearchHistoryEntry
            {
                Value = text,
                TranslatedName = optTranslatedName,
                Time = DateTime.Now
            });
        }

        NavigationView.SelectedItem = null;
    }

    private async void OpenSearchSettingButton_OnClicked(object sender, RoutedEventArgs e)
    {
        await NavigateToSettingEntryAsync(ReverseSearchApiKeyAttribute.Value);
    }

    private async Task NavigateToSettingEntryAsync(SettingsEntryAttribute entry)
    {
        NavigationView.SelectedItem = SettingsTab;
        var settingsPage = await MainPageRootFrame.AwaitPageTransitionAsync<SettingsPage>();
        var panel = settingsPage.ScrollView.ScrollPresenter.Content.To<FrameworkElement>();
        var frameworkElement = panel.FindChild<SettingsCard>(element => element.Tag is SettingsEntryAttribute e && Equals(e, entry));

        if (frameworkElement is not null)
        {
            // ScrollView第一次导航的时候会有一个偏移，等待大小确定后滚动
            await Task.Delay(20);

            var position = frameworkElement
                .TransformToVisual(panel)
                .TransformPoint(new Point(0, 0));

            _ = settingsPage.ScrollView.ScrollTo(position.X, position.Y);
        }
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
                    await _viewModel.ReverseSearchAsync(stream);
            }
        }
    }

    private async void ReverseSearchButton_OnClicked(object sender, RoutedEventArgs e)
    {
        if (App.AppViewModel.AppSettings.ReverseSearchApiKey is { Length: > 0 })
        {
            if (await HWnd.OpenFileOpenPickerAsync() is { } file)
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
        {
            await NavigateToSettingEntryAsync(ReverseSearchApiKeyAttribute.Value);
        }
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

    private async void SelfAvatar_OnTapped(object sender, RoutedEventArgs e)
    {
        await IllustratorViewerHelper.CreateWindowWithPageAsync(App.AppViewModel.PixivUid);
    }

    private void TitleBar_OnPaneButtonClicked(object? sender, RoutedEventArgs e)
    {
        NavigationView.IsPaneOpen = !NavigationView.IsPaneOpen;
    }
}
