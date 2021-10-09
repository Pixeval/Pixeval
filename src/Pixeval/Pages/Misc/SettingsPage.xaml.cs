using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.Controls.IconButton;
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

        private void SubmitExcludeTagButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            AddExcludeTag(ExcludeTagsTextBox.Text);
        }

        private void ExcludeTagsTextBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                AddExcludeTag(ExcludeTagsTextBox.Text);
            }
        }

        private void AddExcludeTag(string text)
        {
            ExcludeTagsTextBox.Text = string.Empty;
            if (_viewModel.ExcludeTags.Contains(text, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            _viewModel.ExcludeTags.Add(text);

            if (_viewModel.ExcludeTags.Contains("R-18", StringComparer.OrdinalIgnoreCase) && _viewModel.ExcludeTags.Contains("R-18G", StringComparer.OrdinalIgnoreCase))
            {
                _viewModel.FiltrateRestrictedContent = true;
            }
        }

        private void ExcludeTagButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var token = ((IconButton) sender).Text;
            if (token.Equals("R-18", StringComparison.OrdinalIgnoreCase) || token.Contains("R-18G", StringComparison.OrdinalIgnoreCase))
            {
                _viewModel.FiltrateRestrictedContent = false;
            }

            _viewModel.ExcludeTags.Remove(token);
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