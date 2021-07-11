using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Pixeval.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pixeval.Pages
{
    public sealed partial class MainPage : Page
    {
        private readonly MainPageViewModel _viewModel = new();

        public MainPage()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void MainPage_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RearrangeMainPageAutoSuggestionBoxMargin();
        }

        private void MainPageRootNavigationView_OnLoaded(object sender, RoutedEventArgs e)
        {
            RearrangeMainPageAutoSuggestionBoxMargin();
        }

        private void MainPageRootNavigationView_OnDisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            RearrangeMainPageAutoSuggestionBoxMargin();
        }

        #region Helper Functions

        private void RearrangeMainPageAutoSuggestionBoxMargin()
        {
            MainPageAutoSuggestionBox.Margin = _viewModel.RearrangeMainPageAutoSuggestionBoxMargin(MainPageRootNavigationView.ActualWidth, MainPageRootNavigationView.DisplayMode switch
            {
                NavigationViewDisplayMode.Minimal  => 0,
                NavigationViewDisplayMode.Compact  => MainPageRootNavigationView.CompactPaneLength,
                NavigationViewDisplayMode.Expanded => MainPageRootNavigationView.OpenPaneLength,
                _                                  => throw new ArgumentOutOfRangeException(nameof(MainPageRootNavigationView))
            });
        }

        #endregion
    }
}
