#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/SettingsPage.xaml.cs
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
using System.Threading.Tasks;
using Windows.System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.Controls.TokenInput;
using Pixeval.Controls.Windowing;
using Pixeval.Database.Managers;
using Pixeval.Download.MacroParser;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using AppContext = Pixeval.AppManagement.AppContext;
using UIElement = Microsoft.UI.Xaml.UIElement;
using Pixeval.Options;
using Pixeval.CoreApi.Global.Enum;
using WinUI3Utilities;

namespace Pixeval.Pages.Misc;

// TODO set language
public sealed partial class SettingsPage
{
    // This TestParser is used to test whether the user input meta path is legal
    private static readonly MacroParser<string> _testParser = new();

    // The previous meta path after user changes the path field, if the path is illegal
    // its value will be reverted to this field.
    private string _previousPath = "";
    private SettingsPageViewModel _viewModel = null!;

    public SettingsPage()
    {
        InitializeComponent();
    }

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        _viewModel = new SettingsPageViewModel(App.AppViewModel.AppSetting, Window);
        _previousPath = _viewModel.DefaultDownloadPathMacro;
    }

    public override void OnPageDeactivated(NavigatingCancelEventArgs e)
    {
        Bindings.StopTracking();
        AppContext.SaveConfig(App.AppViewModel.AppSetting);
        _viewModel = null!;
    }

    private void SettingsPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        CheckForUpdatesEntry.Header = GitVersionInformation.SemVer;
    }

    private void Theme_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        WindowFactory.SetTheme(_viewModel.Theme);
    }

    private void Backdrop_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        WindowFactory.SetBackdrop(_viewModel.Backdrop);
    }

    private async void ThemeEntryDescriptionHyperlinkButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(new Uri("ms-settings:themes"));
    }

    private void ImageMirrorServerTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        ImageMirrorServerTextBoxTeachingTip.IsOpen = false;
    }

    private void ImageMirrorServerTextBox_OnLosingFocus(UIElement sender, LosingFocusEventArgs args)
    {
        if (_viewModel.MirrorHost.IsNotNullOrEmpty() && Uri.CheckHostName(_viewModel.MirrorHost) == UriHostNameType.Unknown)
        {
            ImageMirrorServerTextBox.Text = "";
            ImageMirrorServerTextBoxTeachingTip.IsOpen = true;
        }
    }

    private async void CheckForUpdateButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _viewModel.LastCheckedUpdate = DateTimeOffset.Now;
        CheckingForUpdatePanel.Show();
        // TODO add update check
        await Task.Delay(2000);
        CheckingForUpdatePanel.Collapse();
    }

    private async void FeedbackByEmailHyperlinkButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(new Uri("mailto:decem0730@hotmail.com"));
    }

    private async void OpenFontSettingsHyperlinkButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(new Uri("ms-settings:fonts"));
    }

    private async void PerformSignOutButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (await this.CreateOkCancelAsync(SettingsPageResources.SignOutConfirmationDialogTitle,
                SettingsPageResources.SignOutConfirmationDialogContent) is ContentDialogResult.Primary)
        {
            await AppContext.ClearDataAsync();
            App.AppViewModel.SignOutExit = true;
            Window.Close();
        }
    }

    private async void ResetDefaultSettingsButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (await this.CreateOkCancelAsync(SettingsPageResources.ResetSettingConfirmationDialogTitle,
                SettingsPageResources.ResetSettingConfirmationDialogContent) is ContentDialogResult.Primary)
        {
            _viewModel.ResetDefault();
        }
    }

    private void DefaultDownloadPathMacroTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (_viewModel.DefaultDownloadPathMacro.IsNullOrBlank())
        {
            DownloadMacroInvalidTeachingTip.Subtitle = SettingsPageResources.DownloadMacroInvalidTeachingTipInputCannotBeBlank;
            DownloadMacroInvalidTeachingTip.IsOpen = true;
            _viewModel.DefaultDownloadPathMacro = _previousPath;
            return;
        }

        try
        {
            _testParser.SetupParsingEnvironment(new Lexer(_viewModel.DefaultDownloadPathMacro));
            _ = _testParser.Parse();
        }
        catch (Exception exception)
        {
            DownloadMacroInvalidTeachingTip.Subtitle = SettingsPageResources.DownloadMacroInvalidTeachingTipMacroInvalidFormatted.Format(exception.Message);
            DownloadMacroInvalidTeachingTip.IsOpen = true;
            _viewModel.DefaultDownloadPathMacro = _previousPath;
        }
    }

    private void DefaultDownloadPathMacroTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        DownloadMacroInvalidTeachingTip.IsOpen = false;
        _previousPath = _viewModel.DefaultDownloadPathMacro;
    }

    private void PathMacroTokenInputBox_OnTokenTapped(object? sender, Token e)
    {
        _viewModel.DefaultDownloadPathMacro = _viewModel.DefaultDownloadPathMacro.Insert(DefaultDownloadPathMacroTextBox.SelectionStart, e.TokenContent);
    }

    private void DeleteSearchHistoriesButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var manager = scope.ServiceProvider.GetRequiredService<SearchHistoryPersistentManager>();
        _viewModel.ClearData(ClearDataKind.SearchHistory, manager);
    }

    private void DeleteBrowseHistoriesButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var manager = scope.ServiceProvider.GetRequiredService<BrowseHistoryPersistentManager>();
        _viewModel.ClearData(ClearDataKind.BrowseHistory, manager);
    }

    private void DeleteDownloadHistoriesButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var manager = scope.ServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        _viewModel.ClearData(ClearDataKind.DownloadHistory, manager);
    }

    // ReSharper disable MemberCanBeMadeStatic.Local
    private Type MainPageTabItem => typeof(MainPageTabItem);
    private Type ItemsViewLayoutType => typeof(ItemsViewLayoutType);
    private Type ThumbnailDirection => typeof(ThumbnailDirection);
    private Type IllustrationDownloadFormat => typeof(IllustrationDownloadFormat);
    private Type UgoiraDownloadFormat => typeof(UgoiraDownloadFormat);
    private Type TargetFilter => typeof(TargetFilter);
    private Type BackdropType => typeof(BackdropType);
    private Type SearchTagMatchOption => typeof(SearchTagMatchOption);
    private Type ElementTheme => typeof(ElementTheme);
    private Type IllustrationSortOption => typeof(IllustrationSortOption);
    private Type SearchDuration => typeof(SearchDuration);
    // ReSharper restore MemberCanBeMadeStatic.Local
}
