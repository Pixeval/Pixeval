using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.ViewModel;

namespace Pixeval.Pages
{
    public sealed partial class MainPage
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
