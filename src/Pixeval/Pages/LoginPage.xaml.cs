using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Events;
using Pixeval.Util;
using Pixeval.ViewModel;

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
            try
            {
                if (_viewModel.CheckRefreshAvailable())
                {
                    await _viewModel.RefreshAsync();
                    Frame.Navigate(typeof(MainPage), null, new SlideNavigationTransitionInfo());
                }
                else
                {
                    await _viewModel.WebLoginAsync();
                    Frame.Navigate(typeof(MainPage), null, new SlideNavigationTransitionInfo());
                }

                AppContext.SaveContext();
                await App.PixevalEventChannel.PublishAsync(new LoginCompletedEvent(this, App.PixevalAppClient!.Session));
            }
            catch (Exception exception)
            {
                _ = await MessageDialogBuilder.Create()
                    .WithTitle(LoginPageResources.ErrorWhileLoggingInTitle)
                    .WithContent(string.Format(LoginPageResources.ErrorWhileLogginInContentFormatted, exception.Message))
                    .WithPrimaryButtonText(MessageContentDialogResources.OkButtonContent)
                    .WithDefaultButton(ContentDialogButton.Primary)
                    .Build(this)
                    .ShowAsync();
                Application.Current.Exit();
            }
        }
    }
}