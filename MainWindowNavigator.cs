using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Pixeval.Caching.Persisting;
using Pixeval.Objects;

namespace Pixeval
{
    public partial class MainWindow
    {
        private void SettingsTab_OnSelected(object sender, RoutedEventArgs e)
        {

        }

        private void SignOutTab_OnSelected(object sender, RoutedEventArgs e)
        {
            Identity.Clear();
            var login = new SignIn();
            login.Show();
            Close();
        }

        private void NavigatorList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScheduleHomePage();
        }

        private void ScheduleHomePage()
        {
            if (NavigatorList.SelectedItem is ListViewItem current)
            {
                if (current == MenuTab && !HomeDisplayContainerTransform.Y.Equals(0))
                {
                    HomeDisplayContainerTransform.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(ContentDisplay.ActualHeight, 0, TimeSpan.FromMilliseconds(300)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
                }
                else if (HomeDisplayContainerTransform.Y.Equals(0))
                {
                    HomeDisplayContainerTransform.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(0, 5000, TimeSpan.FromMilliseconds(1500)) {EasingFunction = new CubicEase{EasingMode = EasingMode.EaseOut}});
                }
            }
        }
    }
}