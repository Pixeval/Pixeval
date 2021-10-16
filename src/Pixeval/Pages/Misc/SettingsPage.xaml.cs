using System;
using System.Threading.Tasks;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.Controls.Setting.UI;
using Pixeval.Util;
using Pixeval.Util.UI;
using Pixeval.Utilities;

namespace Pixeval.Pages.Misc
{
    // set language, set main page default selected tab, set font, clear database
    public sealed partial class SettingsPage
    {
        private readonly SettingsPageViewModel _viewModel = new(App.AppViewModel.AppSetting);

        public SettingsPage()
        {
            InitializeComponent();
        }

        private void SettingsPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            CheckForUpdatesEntry.Header = AppContext.AppVersion.ToString();
        }

        private void SingleSelectionSettingEntry_OnSelectionChanged(Controls.Setting.UI.SingleSelectionSettingEntry.SingleSelectionSettingEntry sender, SelectionChangedEventArgs args)
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
    }
}