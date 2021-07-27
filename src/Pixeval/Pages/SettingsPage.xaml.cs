using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Linq;
using Windows.System;
using CommunityToolkit.WinUI.UI.Controls;
using Mako.Global.Enum;
using Pixeval.Util;
using Pixeval.ViewModel;

namespace Pixeval.Pages
{
    public sealed partial class SettingsPage
    {
        private readonly SettingsPageViewModel _viewModel = new(App.AppSetting);

        public SettingsPage()
        {
            InitializeComponent();
        }

        // We use LostFocus event instead of ValueChanged because the ValueChanged event also
        // occurs when the value is a floating-point number and is about to be rounded down to 
        // fall into the valid range, e.g. 100.3
        private void PageLimitForSearchNumberBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            switch (sender)
            {
                case NumberBox {Value: double.NaN} nan:
                    nan.Value = 1;
                    return;
                case NumberBox {Value: > 100 or < 1} invalid:
                    invalid.Value = Objects.CoerceIn(invalid.Value, (1, 100));
                    PageLimitForSearchNumberBoxValueNotInRangeTeachingTip.IsOpen = true;
                    break;
            }
        }

        private void PageLimitForSearchNumberBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            // Close the hint when user is about to type the value
            PageLimitForSearchNumberBoxValueNotInRangeTeachingTip.IsOpen = false;
        }

        // HyperlinkButton.NavigateUri is not working
        private async void ApplicationThemeOpenSystemThemeSettingHyperlinkButton_OnClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings:themes"));
        }

        private void DownloadConcurrencyLevelNumberBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            DownloadConcurrencyLevelValueNotInRangeTeachingTip.IsOpen = false;
        }

        private void DownloadConcurrencyLevelNumberBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            NumberBoxCoerceValueInAndShowTeachingTip(sender, DownloadConcurrencyLevelValueNotInRangeTeachingTip, 1, Environment.ProcessorCount);
        }
        private void SearchStartsFromPageNumberNumberBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            NumberBoxCoerceValueInAndShowTeachingTip(sender, SearchStartsFromPageNumberNumberBoxValueNotInRangeTeachingTip, 1, 150);
        }

        private void SearchStartsFromPageNumberNumberBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            DownloadConcurrencyLevelValueNotInRangeTeachingTip.IsOpen = false;
        }

        private void PageLimitForSpotlightNumberBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            PageLimitForSpotlightNumberBoxValueNotInRangeTeachingTip.IsOpen = false;
        }

        private void PageLimitForSpotlightNumberBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            NumberBoxCoerceValueInAndShowTeachingTip(sender, PageLimitForSpotlightNumberBoxValueNotInRangeTeachingTip, 1, 100);
        }

        private void SwitchSensitiveContentFilteringInfoBarActionButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            SettingsPageRootScrollViewer.ScrollToElement(SensitiveTagTokenizingTextBox);
        }

        private async void SendFeedbackHyperlinkButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("mailto:decem0730@hotmail.com"));
        }

        private void SwitchSensitiveContentFilteringToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
        {
            var toggleSwitch = (ToggleSwitch) sender;
            if (toggleSwitch.IsOn)
            {
                _viewModel.AddR18Filtering();
            }
            else
            {
                _viewModel.RemoveR18Filtering();
            }
        }

        // The RadioButtons.SelectionChanged always returns null, so we have to handle them separately
        // See issue https://github.com/microsoft/microsoft-ui-xaml/issues/3268
        private void ApplicationThemeSystemDefaultRadioButton_OnChecked(object sender, RoutedEventArgs e)
        {
            _viewModel.Theme = ApplicationTheme.SystemDefault;
        }

        private void ApplicationThemeLightRadioButton_OnChecked(object sender, RoutedEventArgs e)
        {
            _viewModel.Theme = ApplicationTheme.Light;
        }

        private void ApplicationThemeDarkRadioButton_OnChecked(object sender, RoutedEventArgs e)
        {
            _viewModel.Theme = ApplicationTheme.Dark;
        }

        private void DefaultSortOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.DefaultSortOption = (IllustrationSortOption) ((ComboBoxItem) DefaultSortOptionComboBox.SelectedItem).Tag;
        }

        private void SearchKeywordMatchOptionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.SearchTagMatchOption = (SearchTagMatchOption) ((ComboBoxItem) SearchKeywordMatchOptionComboBox.SelectedItem).Tag;
        }

        private void SensitiveTagTokenizingTextBox_OnTokenItemAdding(TokenizingTextBox sender, TokenItemAddingEventArgs args)
        {
            if (_viewModel.ExcludeTags.Contains(args.TokenText, StringComparer.OrdinalIgnoreCase))
            {
                args.Cancel = true;
            }
        }

        private void SensitiveTagTokenizingTextBox_OnTokenItemAdded(TokenizingTextBox sender, object args)
        {
            if (_viewModel.ExcludeTags.Contains("R-18", StringComparer.OrdinalIgnoreCase) || _viewModel.ExcludeTags.Contains("R-18G", StringComparer.OrdinalIgnoreCase))
            {
                SwitchSensitiveContentFilteringToggleSwitch.IsOn = true;
            }
        }

        private void SensitiveTagTokenizingTextBox_OnTokenItemRemoved(TokenizingTextBox sender, object args)
        {
            if (!_viewModel.ExcludeTags.Contains("R-18", StringComparer.OrdinalIgnoreCase) || !_viewModel.ExcludeTags.Contains("R-18G", StringComparer.OrdinalIgnoreCase))
            {
                SwitchSensitiveContentFilteringToggleSwitch.IsOn = false;
            }
        }

        private void ImageMirrorServerTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (Uri.CheckHostName(_viewModel.MirrorHost) == UriHostNameType.Unknown && !Uri.IsWellFormedUriString(_viewModel.MirrorHost, UriKind.Absolute))
            {
                ImageMirrorServerTeachingTip.IsOpen = true;
            }
        }

        private void ImageMirrorServerTextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            ImageMirrorServerTeachingTip.IsOpen = false;
        }

        #region Helper Functions

        private object GetApplicationThemeRadioButtonsSelectedItem()
        {
            return ApplicationThemeRadioButtons.Items.Cast<RadioButton>().First(i => i.Tag.Equals(_viewModel.Theme));
        }

        private object GetDefaultSortOptionComboBoxSelectedItem()
        {
            return DefaultSortOptionComboBox.Items.Cast<ComboBoxItem>().First(i => i.Tag.Equals(_viewModel.DefaultSortOption));
        }

        private object GetSearchKeywordMatchOptionComboBoxSelectedItem()
        {
            return SearchKeywordMatchOptionComboBox.Items.Cast<ComboBoxItem>().First(i => i.Tag.Equals(_viewModel.SearchTagMatchOption));
        }

        // TODO: use attached property
        private static void NumberBoxCoerceValueInAndShowTeachingTip(object sender, TeachingTip teachingTip, double startInclusive, double endInclusive)
        {
            switch (sender)
            {
                case NumberBox { Value: double.NaN } nan:
                    nan.Value = startInclusive;
                    return;
                case NumberBox box when !box.Value.InRange((startInclusive, endInclusive)):
                    box.Value = Objects.CoerceIn(box.Value, (startInclusive, endInclusive));
                    teachingTip.IsOpen = true;
                    break;
            }
        }

        #endregion
    }
}
