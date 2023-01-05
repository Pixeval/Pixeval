#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/MainWindow.xaml.cs
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
using System.Runtime.InteropServices;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Misc;
using Pixeval.Pages.Login;
using WinRT.Interop;
using AppContext = Pixeval.AppManagement.AppContext;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Image = SixLabors.ImageSharp.Image;
using WindowSizeChangedEventArgs = Microsoft.UI.Xaml.WindowSizeChangedEventArgs;
using Pixeval.Dialogs;
using Pixeval.Util.UI;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Pixeval.Pages;
using Pixeval.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Attributes;
using Pixeval.Database.Managers;
using Pixeval.Database;
using Pixeval.Messages;
using WinUI3Utilities;

namespace Pixeval;

public sealed partial class MainWindow : INavigationModeInfo
{
    private readonly MainWindowViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        CurrentContext.TitleBar = AppTitleBar;
        CurrentContext.TitleTextBlock = AppTitleTextBlock;
    }

    public NavigationTransitionInfo? DefaultNavigationTransitionInfo { get; internal set; } = new SuppressNavigationTransitionInfo();

    // The parameter of OnNavigatedTo is always NavigationMode.New
    public static NavigationMode? NavigationMode { get; private set; }

    public static NavigationMode? GetNavigationModeAndReset()
    {
        var mode = NavigationMode;
        NavigationMode = null;
        return mode;
    }

    private void PixevalAppRootFrame_OnLoaded(object sender, RoutedEventArgs e)
    {
        PixevalAppRootFrame.Navigate(typeof(LoginPage));
    }

    private void MainWindow_OnClosed(object sender, WindowEventArgs args)
    {
        AppContext.SaveContext();
    }

    private void PixevalAppRootFrame_OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        e.Handled = true;
        throw e.Exception;
    }

    private void PixevalAppRootFrame_OnNavigating(object sender, NavigatingCancelEventArgs e)
    {
        NavigationMode = e.NavigationMode;
    }

    public void ShowProgressRing()
    {
        Processing.Visibility = Visibility.Visible;
    }

    public void HideProgressRing()
    {
        Processing.Visibility = Visibility.Collapsed;
    }

    private void MainWindow_OnSizeChanged(object sender, WindowSizeChangedEventArgs args)
    {

    }

    [DllImport("Shcore.dll", SetLastError = true)]
    internal static extern int GetDpiForMonitor(nint hMonitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY);

    internal enum MonitorDpiType
    {
        MdtEffectiveDpi = 0,
        MdtAngularDpi = 1,
        MdtRawDpi = 2,
        MdtDefault = MdtEffectiveDpi
    }

    private double GetScaleAdjustment()
    {
        var displayArea = DisplayArea.GetFromWindowId(CurrentContext.WindowId, DisplayAreaFallback.Primary);
        var hMonitor = Win32Interop.GetMonitorFromDisplayId(displayArea.DisplayId);

        // Get DPI.
        var result = GetDpiForMonitor(hMonitor, MonitorDpiType.MdtDefault, out var dpiX, out _);
        if (result != 0)
        {
            throw new Exception("Could not get DPI for monitor.");
        }

        var scaleFactorPercent = (uint)(((long)dpiX * 100 + (96 >> 1)) / 96);
        return scaleFactorPercent / 100.0;
    }

    private void SetDragRegionForCustomTitleBar(AppWindow appWindow)
    {
        // Check to see if customization is supported.
        // Currently only supported on Windows 11.
        if (AppWindowTitleBar.IsCustomizationSupported()
            && appWindow.TitleBar.ExtendsContentIntoTitleBar)
        {
            var scaleAdjustment = GetScaleAdjustment();
            RectInt32 dragRectL;
            dragRectL.X = 0;
            dragRectL.Y = 0;
            dragRectL.Width = (int)((LeftDragRegion.ActualWidth + LeftMarginRegion.ActualWidth) * scaleAdjustment);
            dragRectL.Height = (int)(AppTitleBar.ActualHeight * scaleAdjustment);
            RectInt32 dragRectR;
            dragRectR.X = (int)((LeftDragRegion.ActualWidth + LeftMarginRegion.ActualWidth + SearchBarRegion.ActualWidth + MarginRegion.ActualWidth + ReverseSearchButtonRegion.ActualWidth + SearchSettingButtonRegion.ActualWidth) * scaleAdjustment);
            dragRectR.Y = 0;
            dragRectR.Width = (int)(RightDragRegion.ActualWidth * scaleAdjustment);
            dragRectR.Height = (int)(AppTitleBar.ActualHeight * scaleAdjustment);

            appWindow.TitleBar.SetDragRectangles(new[] { dragRectL, dragRectR });
        }
    }

    private void AppTitleBar_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            SetDragRegionForCustomTitleBar(CurrentContext.AppWindow);
        }
    }

    private void AppTitleBar_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            SetDragRegionForCustomTitleBar(CurrentContext.AppWindow);
        }
    }

    private async void KeywordAutoSuggestBox_GotFocus(object sender, RoutedEventArgs e)
    {
        var suggestBox = (AutoSuggestBox)sender;
        suggestBox.IsSuggestionListOpen = true;
        await _viewModel.SuggestionProvider.UpdateAsync(suggestBox.Text);
    }

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

    private static async Task ShowReverseSearchApiKeyNotPresentDialog()
    {
        var content = new ReverseSearchApiKeyNotPresentDialog();
        var dialog = MessageDialogBuilder.Create().WithTitle(MainPageResources.ReverseSearchApiKeyNotPresentTitle)
            .WithContent(content)
            .WithPrimaryButtonText(MessageContentDialogResources.OkButtonContent)
            .WithDefaultButton(ContentDialogButton.Primary)
            .Build(CurrentContext.Window);
        content.Owner = dialog;
        await dialog.ShowAsync();
    }

    private async void KeywordAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        await GoBackToMainPageAsync();
        if (args.QueryText.IsNullOrBlank())
        {
            MessageDialogBuilder.CreateAcknowledgement(this,
                MainPageResources.SearchKeywordCannotBeBlankTitle,
                MainPageResources.SearchKeywordCannotBeBlankContent);
            return;
        }

        switch (args.ChosenSuggestion)
        {
            case SuggestionModel { SuggestionType: SuggestionType.Settings }:
                return;
            case SuggestionModel({ } name, var translatedName, _):
                PerformSearch(name, translatedName);
                break;
            default:
                PerformSearch(args.QueryText);
                break;
        }
    }

    private static void PerformSearch(string text, string? optTranslatedName = null)
    {
        using (var scope = App.AppViewModel.AppServicesScope)
        {
            var manager = scope.ServiceProvider.GetRequiredService<SearchHistoryPersistentManager>();
            // TODO distinguish search illustrations, manga, novels, and users.
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
        WeakReferenceMessenger.Default.Send(new GlobalSearchQuerySubmittedMessage(App.AppViewModel.MakoClient.Search(
            text,
            setting.SearchStartingFromPageNumber,
            setting.PageLimitForKeywordSearch,
            setting.TagMatchOption,
            setting.DefaultSortOption,
            setting.SearchDuration,
            setting.TargetFilter,
            setting.UsePreciseRangeForSearch ? setting.SearchStartDate : null,
            setting.UsePreciseRangeForSearch ? setting.SearchEndDate : null)));
    }

    private async void KeywordAutoSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args.SelectedItem is SuggestionModel { Name: { Length: > 0 } name, SuggestionType: var type })
        {
            await GoBackToMainPageAsync();
            switch (type)
            {
                case SuggestionType.Settings:
                    sender.Text = "";
                    Enum.GetValues<SettingsEntry>().FirstOrNull(se => se.GetLocalizedResourceContent() == name)
                        ?.Let(se => WeakReferenceMessenger.Default.Send(new NavigateToSettingEntryMessage(se)));
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

    private void OpenSearchSettingPopupButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new NavigateToSettingEntryMessage(SettingsEntry.ReverseSearchThreshold));
    }

    private async void ReverseSearchButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (App.AppViewModel.AppSetting.ReverseSearchApiKey is { Length: > 0 })
        {
            if (await UIHelper.OpenFileOpenPickerAsync() is { } file)
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

    private static Task GoBackToMainPageAsync()
    {
        if (App.AppViewModel.AppWindowRootFrame.Content is MainPage)
        {
            return Task.CompletedTask;
        }
        var stack = App.AppViewModel.AppWindowRootFrame.BackStack;
        while (stack.Count >= 1 && stack.Last().SourcePageType != typeof(MainPage))
        {
            stack.RemoveAt(stack.Count - 1);
        }
        App.AppViewModel.AppWindowRootFrame.GoBack();
        return App.AppViewModel.AppWindowRootFrame.AwaitPageTransitionAsync<MainPage>();
    }
}
