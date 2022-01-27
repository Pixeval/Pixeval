#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/SettingsPage.xaml.cs
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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.Controls.Setting.UI;
using Pixeval.Controls.Setting.UI.SingleSelectionSettingEntry;
using Pixeval.Download.MacroParser;
using Pixeval.UserControls.TokenInput;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Pages.Misc;

// TODO set language, clear database
public sealed partial class SettingsPage
{
    // This TestParser is used to test whether the user input meta path is legal
    private static readonly MacroParser<string> TestParser = new();
    private readonly SettingsPageViewModel _viewModel;

    // The previous meta path after user changes the path field, if the path is illegal
    // its value will be reverted to this field.
    private string _previousPath;

    public SettingsPage()
    {
        InitializeComponent();
        _viewModel = new SettingsPageViewModel(App.AppViewModel.AppSetting);
        _previousPath = _viewModel.DefaultDownloadPathMacro;
    }

    private void SettingsPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        CheckForUpdatesEntry.Header = GitVersionInformation.SemVer;
    }

    private void SingleSelectionSettingEntry_OnSelectionChanged(SingleSelectionSettingEntry sender, SelectionChangedEventArgs args)
    {
        App.AppViewModel.SwitchTheme(_viewModel.Theme);
    }

    private async void ThemeEntryDescriptionHyperlinkButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(new Uri("ms-settings:themes"));
    }

    private void ImageMirrorServerTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        ImageMirrorServerTextBoxTeachingTip.IsOpen = false;
    }

    private void ImageMirrorServerTextBox_OnLosingFocus(UIElement sender, LosingFocusEventArgs args)
    {
        if (_viewModel.MirrorHost.IsNotNullOrEmpty() && Uri.CheckHostName(_viewModel.MirrorHost) == UriHostNameType.Unknown)
        {
            ImageMirrorServerTextBox.Text = string.Empty;
            ImageMirrorServerTextBoxTeachingTip.IsOpen = true;
        }
    }

    private async void CheckForUpdateButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _viewModel.LastCheckedUpdate = DateTimeOffset.Now;
        CheckForUpdateButton.Invisible();
        CheckingForUpdatePanel.Visible();
        // TODO add update check
        await Task.Delay(2000);
        CheckForUpdateButton.Visible();
        CheckingForUpdatePanel.Invisible();
    }

    private void FeedbackByEmailHyperlinkButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        Launcher.LaunchUriAsync(new Uri("mailto:decem0730@hotmail.com")).Discard();
    }

    private void OpenFontSettingsHyperlinkButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        Launcher.LaunchUriAsync(new Uri("ms-settings:fonts")).Discard();
    }

    private async void PerformSignOutButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var dialog = MessageDialogBuilder.CreateOkCancel(
            App.AppViewModel.Window,
            SettingsPageResources.SignOutConfirmationDialogTitle,
            SettingsPageResources.SignOutConfirmationDialogContent);
        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            await AppContext.ClearDataAsync();
            App.AppViewModel.SignOutExit = true;
            App.AppViewModel.Window.Close();
        }
    }

    private async void ResetDefaultSettingsButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var dialog = MessageDialogBuilder.CreateOkCancel(
            App.AppViewModel.Window,
            SettingsPageResources.ResetSettingConfirmationDialogTitle,
            SettingsPageResources.ResetSettingConfirmationDialogContent);
        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            _viewModel.ResetDefault();
        }
    }

    private void DefaultDownloadPathMacroTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (_viewModel.DefaultDownloadPathMacro.IsNullOrBlank())
        {
            DownloadPathMacroInvalidTeachingTip.IsOpen = true;
            DownloadPathMacroInvalidTeachingTip.Subtitle = SettingsPageResources.DownloadPathMacroInvalidTeachingTipInputCannotBeBlank;
            _viewModel.DefaultDownloadPathMacro = _previousPath;
            return;
        }

        try
        {
            TestParser.SetupParsingEnvironment(new Lexer(_viewModel.DefaultDownloadPathMacro));
            TestParser.Parse();
        }
        catch (Exception exception)
        {
            DownloadPathMacroInvalidTeachingTip.IsOpen = true;
            DownloadPathMacroInvalidTeachingTip.Subtitle = SettingsPageResources.DownloadPathMacroInvalidTeachingTipMacroInvalidFormatted.Format(exception.Message);
            _viewModel.DefaultDownloadPathMacro = _previousPath;
        }
    }

    private void DefaultDownloadPathMacroTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        DownloadPathMacroInvalidTeachingTip.IsOpen = false;
        _previousPath = _viewModel.DefaultDownloadPathMacro;
    }

    private void MacroTokenInputBox_OnTokenTapped(object? sender, Token e)
    {
        _viewModel.DefaultDownloadPathMacro = _viewModel.DefaultDownloadPathMacro.Insert(DefaultDownloadPathMacroTextBox.SelectionStart, e.TokenContent);
    }
}