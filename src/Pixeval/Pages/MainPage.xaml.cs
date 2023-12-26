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
using System.Runtime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppLifecycle;
using Pixeval.Activation;
using Pixeval.Controls;
using Pixeval.Controls.IllustrationView;
using Pixeval.Database;
using Pixeval.Database.Managers;
using Pixeval.Dialogs;
using Pixeval.Download;
using Pixeval.Messages;
using Pixeval.Pages.Capability;
using Pixeval.Pages.Download;
using Pixeval.Pages.Misc;
using Pixeval.Util;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities;
using Image = SixLabors.ImageSharp.Image;

namespace Pixeval.Pages;

public sealed partial class MainPage : SupportCustomTitleBarDragRegionPage
{
    private static UIElement? _connectedAnimationTarget;

    // This field contains the view model that the illustration viewer is
    // currently holding if we're navigating back to the MainPage
    private static IllustrationItemViewModel? _illustrationViewerContent;

    private readonly MainPageViewModel _viewModel;

    public MainPage()
    {
        _viewModel = new MainPageViewModel(this);
        InitializeComponent();
        CurrentContext.TitleBar = TitleBarGrid;
        CurrentContext.TitleTextBlock = AppTitleTextBlock;
        DataContext = _viewModel;
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            MainPageRootNavigationView.PaneCustomContent = null;
        }
    }

    protected override void SetTitleBarDragRegion(InputNonClientPointerSource sender, SizeInt32 windowSize, double scaleFactor, out int titleBarHeight)
    {
        titleBarHeight = 48;
        // NavigationView的Pane按钮
        var leftIndent = new RectInt32(0, 0, titleBarHeight, titleBarHeight);
        sender.SetRegionRects(NonClientRegionKind.Icon, [FromControl(Icon)]);
        sender.SetRegionRects(NonClientRegionKind.Passthrough, [FromControl(TitleBarControlGrid), GetScaledRect(leftIndent)]);
    }

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        App.AppViewModel.AppLoggedIn();

        // dirty trick, the order of the menu items is the same as the order of the fields in MainPageTabItem
        // since enums are basically integers, we just need a cast to transform it to the correct offset.
        ((NavigationViewItem)MainPageRootNavigationView.MenuItems[(int)App.AppViewModel.AppSetting.DefaultSelectedTabItem]).IsSelected = true;

        // The application is invoked by a protocol, call the corresponding protocol handler.
        if (App.AppViewModel.ConsumeProtocolActivation())
        {
            ActivationRegistrar.Dispatch(AppInstance.GetCurrent().GetActivatedEventArgs());
        }

        _ = WeakReferenceMessenger.Default.TryRegister<MainPage, MainPageFrameSetConnectedAnimationTargetMessage>(this, (_, message) => _connectedAnimationTarget = message.Sender);
        _ = WeakReferenceMessenger.Default.TryRegister<MainPage, NavigatingBackToMainPageMessage>(this, (_, message) => _illustrationViewerContent = message.IllustrationViewModel);
        _ = WeakReferenceMessenger.Default.TryRegister<MainPage, IllustrationTagClickedMessage>(this, (_, message) => PerformSearch(message.Tag));
        _ = WeakReferenceMessenger.Default.TryRegister<MainPage, GlobalSearchQuerySubmittedMessage>(this, (_1, message) =>
        {
            MainPageRootNavigationView.SelectedItem = null;
            _ = MainPageRootFrame.Navigate(typeof(SearchResultsPage), message.Parameter);
        });
        _ = WeakReferenceMessenger.Default.TryRegister<MainPage, NavigateToSettingEntryMessage>(this, (_, message) => NavigateToSettingEntryAsync(message.Entry).Discard());

        // Connected animation to the element located in MainPage
        if (ConnectedAnimationService.GetForCurrentView().GetAnimation("ForwardConnectedAnimation") is { } animation)
        {
            animation.Configuration = new DirectConnectedAnimationConfiguration();
            _ = animation.TryStart(_connectedAnimationTarget ?? this);
            _connectedAnimationTarget = null;
        }

        // TODO: Scroll the content to the item that were being browsed just now
        //if (_illustrationViewerContent is not null && MainPageRootFrame.FindDescendant<ItemsRepeater>() is { } gridView)
        //{
        //    gridView.ScrollIntoView(_illustrationViewerContent);
        //    _illustrationViewerContent = null;
        //}
    }

    private void MainPageRootNavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        // The App.AppViewModel.IllustrationDownloadManager will be initialized after that of MainPage object
        // so we cannot put a navigation tag inside MainPage and treat it as a field, since it will be initialized immediately after
        // the creation of the object while the App.AppViewModel.IllustrationDownloadManager is still null which
        // will lead the program into NullReferenceException on the access of QueuedTasks.

        // args.SelectedItem may be null here
        if (Equals(args.SelectedItem, DownloadListTab))
        {
            Navigate<DownloadListPage>(MainPageRootFrame, null);
            return;
        }

        MainPageRootFrame.NavigateByNavigationViewTag(sender, new SuppressNavigationTransitionInfo());
    }

    private void MainPageRootFrame_OnNavigated(object sender, NavigationEventArgs e)
    {
        _ = WeakReferenceMessenger.Default.Send(new MainPageFrameNavigatingEvent(this));
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect();
    }

    private async void KeywordAutoSuggestBox_GotFocus(object sender, RoutedEventArgs e)
    {
        var suggestBox = (AutoSuggestBox)sender;
        suggestBox.IsSuggestionListOpen = true;
        await _viewModel.SuggestionProvider.UpdateAsync(suggestBox.Text);
    }

    // 搜索并跳转至搜索结果
    private void KeywordAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.QueryText.IsNullOrBlank())
        {
            _ = this.CreateAcknowledgement(MainPageResources.SearchKeywordCannotBeBlankTitle,
                MainPageResources.SearchKeywordCannotBeBlankContent);
            return;
        }

        switch (args.ChosenSuggestion)
        {
            case SuggestionModel { SuggestionType: SuggestionType.Settings or SuggestionType.IllustrationAutoCompleteTagHeader or SuggestionType.IllustrationTrendingTagHeader or SuggestionType.NovelTrendingTagHeader or SuggestionType.SettingEntryHeader }:
                return;
            case SuggestionModel({ } name, var translatedName, _):
                PerformSearch(name, translatedName);
                break;
            default:
                PerformSearch(args.QueryText);
                break;
        }
    }

    private void KeywordAutoSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args.SelectedItem is SuggestionModel { Name: { Length: > 0 } name, SuggestionType: var type })
        {
            switch (type)
            {
                case SuggestionType.Settings:
                    _ = (SettingEntry.LazyValues.Value.FirstOrDefault(se => se.GetLocalizedResourceContent() == name)
                        ?.Let(se => WeakReferenceMessenger.Default.Send(new NavigateToSettingEntryMessage(se))));
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

    private void PerformSearch(string text, string? optTranslatedName = null)
    {
        using (var scope = App.AppViewModel.AppServicesScope)
        {
            var manager = scope.ServiceProvider.GetRequiredService<SearchHistoryPersistentManager>();
            if (manager.Count == 0 || manager.Select(count: 1).AsList() is [{ Value: var last }, ..] && last != text)
            {
                manager.Insert(new SearchHistoryEntry
                {
                    Value = text,
                    TranslatedName = optTranslatedName,
                    Time = DateTime.Now
                });
            }
        }

        var setting = App.AppViewModel.AppSetting;
        MainPageRootNavigationView.SelectedItem = null;
        _ = MainPageRootFrame.Navigate(typeof(SearchResultsPage), App.AppViewModel.MakoClient.Search(
            text,
            setting.SearchStartingFromPageNumber,
            setting.PageLimitForKeywordSearch,
            setting.TagMatchOption,
            setting.DefaultSortOption,
            setting.SearchDuration,
            setting.TargetFilter,
            setting.UsePreciseRangeForSearch ? setting.SearchStartDate : null,
            setting.UsePreciseRangeForSearch ? setting.SearchEndDate : null));
    }

    private void OpenSearchSettingButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        NavigateToSettingEntryAsync(SettingEntry.ReverseSearchResultSimilarityThreshold).Discard();
    }

    private async Task NavigateToSettingEntryAsync(SettingEntry entry)
    {
        MainPageRootNavigationView.SelectedItem = SettingsTab;
        await MainPageRootFrame.AwaitPageTransitionAsync<SettingsPage>();
        var settingsPage = MainPageRootFrame.FindDescendant<SettingsPage>()!;
        var position = settingsPage.FindChild<FrameworkElement>(element => element.Tag is SettingEntry e && e == entry)
            ?.TransformToVisual((UIElement)settingsPage.SettingsPageScrollViewer.Content)
            .TransformPoint(new Point(0, 0));
        _ = settingsPage.SettingsPageScrollViewer.ChangeView(null, position?.Y, null, false);
    }

    // The AutoSuggestBox does not have a 'Paste' event, so we check the keyboard event accordingly
    private async void KeywordAutoSuggestionBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.LeftControl).HasFlag(CoreVirtualKeyStates.Down) && e.Key == VirtualKey.V)
        {
            var content = Clipboard.GetContent();
            if (content.AvailableFormats.Contains(StandardDataFormats.StorageItems) &&
                (await content.GetStorageItemsAsync()).FirstOrDefault(i => i.IsOfType(StorageItemTypes.File)) is StorageFile file)
            {
                e.Handled = true; // prevent the event from bubbling if it contains an image, since it means that we want to do reverse search.
                await using var stream = await file.OpenStreamForReadAsync();
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (await Image.DetectFormatAsync(stream) is not null)
                {
                    if (App.AppViewModel.AppSetting.ReverseSearchApiKey is not { Length: > 0 })
                    {
                        await ShowReverseSearchApiKeyNotPresentDialog();
                        return;
                    }

                    await _viewModel.ReverseSearchAsync(stream);
                }
            }
        }
    }

    private async void ReverseSearchButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (App.AppViewModel.AppSetting.ReverseSearchApiKey is { Length: > 0 })
        {
            if (await Window.OpenFileOpenPickerAsync() is { } file)
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

    private async Task ShowReverseSearchApiKeyNotPresentDialog()
    {
        var content = new ReverseSearchApiKeyNotPresentDialog();
        var dialog = this.CreateAcknowledgement(MainPageResources.ReverseSearchApiKeyNotPresentTitle, content);
        content.Owner = dialog;
        _ = await dialog.ShowAsync();
    }

    private void KeywordAutoSuggestBox_OnDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private async void KeywordAutoSuggestBox_OnDrop(object sender, DragEventArgs e)
    {
        if (App.AppViewModel.AppSetting.ReverseSearchApiKey is { Length: > 0 })
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
}
