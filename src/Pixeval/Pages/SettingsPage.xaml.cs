using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Linq;
using Windows.System;
using CommunityToolkit.WinUI.UI.Controls;
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

        #region TeachingTips Got and Lost Focus

        private void PageLimitForSearchNumberBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            // Close the hint when user is about to type the value
            PageLimitForSearchNumberBoxValueNotInRangeTeachingTip.IsOpen = false;
        }

        // We use LostFocus event instead of ValueChanged because the ValueChanged event also
        // occurs when the value is a floating-point number and is about to be rounded down to 
        // fall into the valid range, e.g. 100.3
        private void PageLimitForSearchNumberBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            NumberBoxCoerceValueInAndShowTeachingTip(sender, PageLimitForSpotlightNumberBoxValueNotInRangeTeachingTip, 1, 100);
        }

        private void DownloadConcurrencyLevelNumberBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            DownloadConcurrencyLevelValueNotInRangeTeachingTip.IsOpen = false;
        }

        private void DownloadConcurrencyLevelNumberBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            NumberBoxCoerceValueInAndShowTeachingTip(sender, DownloadConcurrencyLevelValueNotInRangeTeachingTip, 1, Environment.ProcessorCount);
        }

        private void SearchStartsFromPageNumberNumberBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            DownloadConcurrencyLevelValueNotInRangeTeachingTip.IsOpen = false;
        }

        private void SearchStartsFromPageNumberNumberBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            NumberBoxCoerceValueInAndShowTeachingTip(sender, SearchStartsFromPageNumberNumberBoxValueNotInRangeTeachingTip, 1, 150);
        }

        private void PageLimitForSpotlightNumberBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            PageLimitForSpotlightNumberBoxValueNotInRangeTeachingTip.IsOpen = false;
        }

        private void PageLimitForSpotlightNumberBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            NumberBoxCoerceValueInAndShowTeachingTip(sender, PageLimitForSpotlightNumberBoxValueNotInRangeTeachingTip, 1, 100);
        }

        #endregion

        #region Application Theme Switch

        // HyperlinkButton.NavigateUri is not working
        private async void ApplicationThemeOpenSystemThemeSettingHyperlinkButton_OnClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings:themes"));
        }

        #endregion

        #region Sensitive Tags

        private void SwitchSensitiveContentFilteringToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
        {
            var toggleSwitch = (ToggleSwitch)sender;
            if (toggleSwitch.IsOn)
            {
                _viewModel.AddR18Filtering();
            }
            else
            {
                _viewModel.RemoveR18Filtering();
            }
        }

        private void SwitchSensitiveContentFilteringInfoBarActionButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            SettingsPageRootScrollViewer.ScrollToElement(SensitiveTagTokenizingTextBox);
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

        private void SensitiveTagTokenizingTextBox_OnTokenItemAdding(TokenizingTextBox sender, TokenItemAddingEventArgs args)
        {
            if (_viewModel.ExcludeTags.Contains(args.TokenText, StringComparer.OrdinalIgnoreCase))
            {
                args.Cancel = true;
            }
        }

        #endregion

        #region Image Mirror Server

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

        #endregion

        private async void SendFeedbackHyperlinkButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("mailto:decem0730@hotmail.com"));
        }

        // We cannot use RadioButtons.SelectionChanged since it always returns null
        // see https://github.com/microsoft/microsoft-ui-xaml/issues/3268
        private void ApplicationThemeRadioButton_OnChecked(object sender, RoutedEventArgs e)
        {
            _viewModel.Theme = sender.GetDataContext<ApplicationTheme>();
        }

        #region Helper Functions

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
