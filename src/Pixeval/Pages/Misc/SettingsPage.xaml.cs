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
using Windows.System;
using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.Database.Managers;
using Pixeval.Download.MacroParser;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Pixeval.Options;
using WinUI3Utilities;

namespace Pixeval.Pages.Misc;

[INotifyPropertyChanged]
public sealed partial class SettingsPage
{
    /// <summary>
    /// This TestParser is used to test whether the user input meta path is legal
    /// </summary>
    private static readonly MacroParser<string> _testParser = new();

    /// <summary>
    /// The previous meta path after user changes the path field, if the path is illegal
    /// its value will be reverted to this field.
    /// </summary>
    private string _previousPath = "";

    private SettingsPageViewModel ViewModel { get; set; } = null!;

    public SettingsPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        ViewModel = new SettingsPageViewModel(Window.Content.To<FrameworkElement>());
        _previousPath = ViewModel.DefaultDownloadPathMacro;
    }

    private void SettingsPage_OnUnloaded(object sender, RoutedEventArgs e)
    {
        Bindings.StopTracking();
        ViewModel.SaveCollections();
        App.AppViewModel.AppSettings = ViewModel.AppSetting;
        AppInfo.SaveConfig(ViewModel.AppSetting);
        ViewModel.Dispose();
        ViewModel = null!;
    }

    private void Theme_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        WindowFactory.SetTheme(ViewModel.Theme);
    }

    private void Backdrop_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        WindowFactory.SetBackdrop(ViewModel.Backdrop);
    }

    private void ImageMirrorServerTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        ImageMirrorServerTextBoxTeachingTip.IsOpen = false;
    }

    private void ImageMirrorServerTextBox_OnLosingFocus(UIElement sender, LosingFocusEventArgs args)
    {
        if (ViewModel.MirrorHost.IsNotNullOrEmpty() && Uri.CheckHostName(ViewModel.MirrorHost) is UriHostNameType.Unknown)
        {
            ImageMirrorServerTextBox.Text = "";
            ImageMirrorServerTextBoxTeachingTip.IsOpen = true;
        }
    }

    private void CheckForUpdateButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel.CheckForUpdate();
    }

    private async void OpenHyperlinkButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(new Uri(sender.GetTag<string>()));
    }

    private async void ReleaseNotesHyperlink_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = await this.CreateAcknowledgementAsync(SettingsPageResources.ReleaseNotesHyperlinkButtonContent,
            new ScrollView
            {
                Content = new MarkdownTextBlock
                {
                    Config = new MarkdownConfig(),
                    Text = (sender.GetTag<string>() is "Newest"
                        ? AppInfo.AppVersion.NewestAppReleaseModel
                        : AppInfo.AppVersion.CurrentAppReleaseModel)?.ReleaseNote ?? ""
                }
            });
    }

    private async void PerformSignOutButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (await this.CreateOkCancelAsync(SettingsPageResources.SignOutConfirmationDialogTitle,
                SettingsPageResources.SignOutConfirmationDialogContent) is ContentDialogResult.Primary)
        {
            AppInfo.ClearSession();
            App.AppViewModel.SignOutExit = true;
            Window.Close();
        }
    }

    private async void ResetDefaultSettingsButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (await this.CreateOkCancelAsync(SettingsPageResources.ResetSettingConfirmationDialogTitle,
                SettingsPageResources.ResetSettingConfirmationDialogContent) is ContentDialogResult.Primary)
        {
            ViewModel.ResetDefault();
            OnPropertyChanged(nameof(ViewModel));
        }
    }

    private void DefaultDownloadPathMacroTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (ViewModel.DefaultDownloadPathMacro.IsNullOrBlank())
        {
            DownloadMacroInvalidTeachingTip.Subtitle = SettingsPageResources.DownloadMacroInvalidTeachingTipInputCannotBeBlank;
            DownloadMacroInvalidTeachingTip.IsOpen = true;
            ViewModel.DefaultDownloadPathMacro = _previousPath;
            return;
        }

        try
        {
            _testParser.SetupParsingEnvironment(new Lexer(ViewModel.DefaultDownloadPathMacro));
            _ = _testParser.Parse();
        }
        catch (Exception exception)
        {
            DownloadMacroInvalidTeachingTip.Subtitle = SettingsPageResources.DownloadMacroInvalidTeachingTipMacroInvalidFormatted.Format(exception.Message);
            DownloadMacroInvalidTeachingTip.IsOpen = true;
            ViewModel.DefaultDownloadPathMacro = _previousPath;
        }
    }

    private void DefaultDownloadPathMacroTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        DownloadMacroInvalidTeachingTip.IsOpen = false;
        _previousPath = ViewModel.DefaultDownloadPathMacro;
    }

    private void PathMacroTokenInputBox_OnTokenTapped(object? sender, Token e)
    {
        ViewModel.DefaultDownloadPathMacro = ViewModel.DefaultDownloadPathMacro.Insert(DefaultDownloadPathMacroTextBox.SelectionStart, e.TokenContent);
    }

    private void DeleteSearchHistoriesButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var manager = scope.ServiceProvider.GetRequiredService<SearchHistoryPersistentManager>();
        ViewModel.ClearData(ClearDataKind.SearchHistory, manager);
    }

    private void DeleteBrowseHistoriesButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var manager = scope.ServiceProvider.GetRequiredService<BrowseHistoryPersistentManager>();
        ViewModel.ClearData(ClearDataKind.BrowseHistory, manager);
    }

    private void DeleteDownloadHistoriesButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var manager = scope.ServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        ViewModel.ClearData(ClearDataKind.DownloadHistory, manager);
    }

    private void OpenFolder_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var folder = sender.GetTag<string>() switch
        {
            "Log" => AppKnownFolders.Log.Self,
            "Temp" => AppKnownFolders.Temporary.Self,
            "Local" => AppKnownFolders.Local.Self,
            "Roaming" => AppKnownFolders.Roaming.Self,
            _ => null
        };
        if (folder is not null)
            _ = Launcher.LaunchFolderAsync(folder);
    }
}
