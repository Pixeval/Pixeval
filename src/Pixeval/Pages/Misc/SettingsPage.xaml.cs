using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.Controls.Setting.UI;
using Pixeval.Controls.Setting.UI.SingleSelectionSettingEntry;
using Pixeval.Util.UI;
using Pixeval.Utilities;

namespace Pixeval.Pages.Misc
{
    // TODO: check for update, set language, set main page default selected tab
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

        private void ExcludeTagsTokenTextBox_OnTokenItemAdded(TokenizingTextBox sender, object args)
        {
            if (_viewModel.ExcludeTags.Contains("R-18", StringComparer.OrdinalIgnoreCase) || _viewModel.ExcludeTags.Contains("R-18G", StringComparer.OrdinalIgnoreCase))
            {
                FiltrateRestrictedContentSwitch.IsOn = true;
            }
        }

        private void ExcludeTagsTokenTextBox_OnTokenItemRemoved(TokenizingTextBox sender, object args)
        {
            if (!_viewModel.ExcludeTags.Contains("R-18", StringComparer.OrdinalIgnoreCase) || !_viewModel.ExcludeTags.Contains("R-18G", StringComparer.OrdinalIgnoreCase))
            {
                FiltrateRestrictedContentSwitch.IsOn = false;
            }
        }

        private void ExcludeTagsTokenTextBox_OnTokenItemAdding(TokenizingTextBox sender, TokenItemAddingEventArgs args)
        {
            if (_viewModel.ExcludeTags.Contains(args.TokenText, StringComparer.OrdinalIgnoreCase))
            {
                args.Cancel = true;
            }
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
            await Task.Delay(2000);
            CheckForUpdateButton.Visible();
            CheckingForUpdatePanel.Invisible();
        }

        private void FeedbackByEmailHyperlinkButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _ = Launcher.LaunchUriAsync(new Uri("mailto:decem0730@hotmail.com"));
        }
    }
}
