#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/MainWindow.xaml.cs
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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.AppManagement;
using Pixeval.Misc;
using Pixeval.Pages.Misc;

namespace Pixeval
{
    public sealed partial class MainWindow : INavigationModeInfo
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public NavigationTransitionInfo? DefaultNavigationTransitionInfo { get; internal set; } =
            new SuppressNavigationTransitionInfo();

        // Remarks: The parameter of OnNavigatedTo is always NavigationMode.New
        public NavigationMode? NavigationMode { get; private set; }

        public NavigationMode? GetNavigationModeAndReset()
        {
            var mode = NavigationMode;
            NavigationMode = null;
            return mode;
        }

        private void PixevalAppRootFrame_OnLoaded(object sender, RoutedEventArgs e)
        {
            PixevalAppRootFrame.Navigate(typeof(LoginPage));
        }

        private void MainWindow_OnClosed(object sender, WindowEventArgs args)
        {
            AppContext.SaveContext();
        }

        private void PixevalAppRootFrame_OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            e.Handled = true;
            throw e.Exception;
        }

        private void PixevalAppRootFrame_OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            NavigationMode = e.NavigationMode;
        }

        public void ShowActivationProgressRing()
        {
            ProcessingActivation.Visibility = Visibility.Visible;
        }

        public void HideActivationProgressRing()
        {
            ProcessingActivation.Visibility = Visibility.Collapsed;
        }
    }
}