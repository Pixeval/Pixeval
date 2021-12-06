#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/MainPage.xaml.cs
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
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using Windows.Foundation;
using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppLifecycle;
using Pixeval.Activation;
using Pixeval.Download;
using Pixeval.Messages;
using Pixeval.Pages.Capability;
using Pixeval.Pages.Download;
using Pixeval.Pages.Misc;
using Pixeval.UserControls;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using AppContext = Pixeval.AppManagement.AppContext;
using Pixeval.Database;
using Pixeval.Database.Managers;
using Microsoft.Extensions.DependencyInjection;

namespace Pixeval.Pages
{
    public sealed partial class MainPage
    {
        private static UIElement? _connectedAnimationTarget;

        // This field contains the view model that the illustration viewer is
        // currently holding if we're navigating back to the MainPage
        private static IllustrationViewModel? _illustrationViewerContent;

        private readonly MainPageViewModel _viewModel = new();
        public MainPage()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        public override void OnPageDeactivated(NavigatingCancelEventArgs e)
        {
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }

        public override void OnPageActivated(NavigationEventArgs e)
        {
            // dirty trick, the order of the menu items is the same as the order of the fields in MainPageTabItem
            // since enums are basically integers, we just need a cast to transform it to the correct offset.
            ((NavigationViewItem) MainPageRootNavigationView.MenuItems[(int) App.AppViewModel.AppSetting.DefaultSelectedTabItem]).IsSelected = true;


            if (App.AppViewModel.ConsumeProtocolActivation())
            {
                ActivationRegistrar.Dispatch(AppInstance.GetCurrent().GetActivatedEventArgs());
            }

            WeakReferenceMessenger.Default.Register<MainPage, MainPageFrameSetConnectedAnimationTargetMessage>(this, (_, message) => _connectedAnimationTarget = message.Sender);
            WeakReferenceMessenger.Default.Register<MainPage, NavigatingBackToMainPageMessage>(this, (_, message) => _illustrationViewerContent = message.IllustrationViewModel);
            WeakReferenceMessenger.Default.Register<MainPage, IllustrationTagClickedMessage>(this, async (_, message) => await PerformSearchAsync(message.Tag));

            // Connected animation to the element located in MainPage
            if (ConnectedAnimationService.GetForCurrentView().GetAnimation("ForwardConnectedAnimation") is { } animation)
            {
                animation.Configuration = new DirectConnectedAnimationConfiguration();
                animation.TryStart(_connectedAnimationTarget ?? this);
                _connectedAnimationTarget = null;
            }

            // Scroll the content to the item that were being browsed just now
            if (_illustrationViewerContent is not null && MainPageRootFrame.FindDescendant<AdaptiveGridView>() is { } gridView)
            {
                gridView.ScrollIntoView(_illustrationViewerContent);
                _illustrationViewerContent = null;
            }
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
                MainPageRootFrame.Navigate(typeof(DownloadListPage), App.AppViewModel.DownloadManager.QueuedTasks.Where(task => task is not IIntrinsicDownloadTask));
                return;
            }
            MainPageRootFrame.NavigateByNavigationViewTag(sender, new SuppressNavigationTransitionInfo());
        }

        private void MainPageRootFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new MainPageFrameNavigatingEvent(this));
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
        }
        
        private async void KeywordAutoSuggestBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var suggestBox = (AutoSuggestBox) sender;
            suggestBox.IsSuggestionListOpen = true;

            if (!_viewModel.Suggestions.Any()) 
                await _viewModel.AppendSearchHistoryAsync(); // Show search history
        }

        // 搜索并跳转至搜索结果
        private async void KeywordAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.QueryText.IsNullOrBlank())
            {
                UIHelper.ShowTextToastNotification(
                    MainPageResources.SearchKeywordCannotBeBlankToastTitle,
                    MainPageResources.SearchKeywordCannotBeBlankToastContent,
                    AppContext.AppLogoNoCaptionUri);
                return;
            }

            await PerformSearchAsync(args.QueryText);
        }

        private void KeywordAutoSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Text = (args.SelectedItem as SuggestionModel)?.Name;
        }

        private async void KeywordAutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var items = _viewModel.Suggestions;

            if (sender.Text is { Length: > 0 } keyword)
            {
                var suggestions = (await App.AppViewModel.MakoClient.GetAutoCompletionForKeyword(keyword)).Select(SuggestionModel.FromTag);
                items.ReplaceByUpdate(suggestions);
            }
            else
            {
                // Clear the suggestions when there is nothing to search
                items.Clear();

                // Show search history
                await _viewModel.AppendSearchHistoryAsync();
            }
        }

        private async Task PerformSearchAsync(string text)
        {
            using (var scope = App.AppViewModel.AppServicesScope)
            {
                var manager = await scope.ServiceProvider.GetRequiredService<Task<SearchHistoryPersistentManager>>();
                await manager.InsertAsync(new SearchHistoryEntry
                {
                    Value = text,
                    Time = DateTime.Now,
                });
            }

            var setting = App.AppViewModel.AppSetting;
            MainPageRootNavigationView.SelectedItem = null;
            MainPageRootFrame.Navigate(typeof(SearchResultsPage), App.AppViewModel.MakoClient.Search(
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

        private async void OpenSearchSettingPopupButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            MainPageRootNavigationView.SelectedItem = SettingsTab;
            WeakReferenceMessenger.Default.Send(new OpenSearchSettingMessage());
            // The stupid delay here does merely nothing but wait the navigation to complete, apparently
            // the navigation is asynchronous and there's no way to wait for it
            await Task.Delay(500);
            var settingsPage = MainPageRootFrame.FindDescendant<SettingsPage>()!;
            var position = settingsPage.SearchSettingsGroup
                .TransformToVisual((UIElement) settingsPage.SettingsPageScrollViewer.Content)
                .TransformPoint(new Point(0, 0));
            settingsPage.SettingsPageScrollViewer.ChangeView(null, position.Y, null, false);
        }
    }
}