using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.Controls.Timeline;
using WinUI3Utilities;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pixeval.Pages.Capability.Feeds
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FeedPage
    {
        public FeedPage()
        {
            InitializeComponent();
            _viewModel = new FeedPageViewModel();
        }

        private readonly FeedPageViewModel _viewModel;

        private void FeedPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel.DataProvider.ResetEngine(App.AppViewModel.MakoClient.Feeds()!);
        }

        private async void TimelineUnit_OnLoaded(object sender, RoutedEventArgs e)
        {
            var vm = (sender as TimelineUnit)!.GetDataContext<FeedItemViewModel>();
            await vm.LoadAsync();
        }
    }
}
