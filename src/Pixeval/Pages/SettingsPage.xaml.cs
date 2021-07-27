using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using Windows.System;
using Pixeval.Util;

namespace Pixeval.Pages
{
    public sealed partial class SettingsPage
    {
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
            switch (sender)
            {
                case NumberBox { Value: double.NaN } nan:
                    nan.Value = 1;
                    return;
                case NumberBox box when !box.Value.InRange((1, Environment.ProcessorCount)):
                    box.Value = Objects.CoerceIn(box.Value, (1, Environment.ProcessorCount));
                    DownloadConcurrencyLevelValueNotInRangeTeachingTip.IsOpen = true;
                    break;
            }
        }

        private void SwitchSensitiveContentFilteringInfoBarActionButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            SettingsPageRootScrollViewer.ScrollToElement(SensitiveTagTokenizingTextBox);
        }

        private async void SendFeedbackHyperlinkButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("mailto:decem0730@hotmail.com"));
        }
    }
}
