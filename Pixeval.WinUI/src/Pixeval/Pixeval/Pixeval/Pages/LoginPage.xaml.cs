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
using Pixeval.Util;
using Pixeval.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pixeval.Pages
{
    public sealed partial class LoginPage
    {
        private readonly LoginPageViewModel _viewModel = new();

        public LoginPage()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private async void LoginPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            _ = await MessageDialogBuilder.Create()
                .WithTitle(UIHelper.GetLocalizedString("RefreshingSessionIsNotPresentTitle"))
                .WithContent(UIHelper.GetLocalizedString("RefreshingSessionIsNotPresentContent"))
                .WithPrimaryButtonText(UIHelper.GetLocalizedString("OkButtonContent"))
                .WithDefaultButton(ContentDialogButton.Primary)
                .Build()
                .ShowAsync();
            Directory.Delete(AppContext.AppConfigurationFolder, true);
            Application.Current.Exit();
            _viewModel.RefreshAvailable = await _viewModel.CheckRefreshAvailable();
            if (_viewModel.RefreshAvailable)
            {
                await _viewModel.Refresh();
                Frame.Navigate(typeof(MainPage), null, new SlideNavigationTransitionInfo());
            }
            else
            {

            }
        }

        private void PrepareForLogin()
        {

        }
    }
}
