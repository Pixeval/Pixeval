using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Controls.IllustratorView;
using Pixeval.CoreApi.Global.Enum;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pixeval.Pages.Capability
{
    public sealed partial class FollowingsPage
    {
        private FollowingsPageViewModel _viewModel = new();


        public FollowingsPage()
        {
            InitializeComponent();
        }


        private async void FollowingsPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadFollowings();
        }

        private void IllustratorView_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is IllustratorView view)
            {
                App.AppViewModel.RootFrameNavigate(typeof(IllustratorPage), view.ViewModel, new SlideNavigationTransitionInfo
                {
                    Effect = SlideNavigationTransitionEffect.FromRight
                });
            }
        }
    }
}
