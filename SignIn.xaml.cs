using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using Pixeval.Caching.Persisting;
using Pixeval.Objects;
using Pixeval.Objects.Exceptions;
using Refit;

namespace Pixeval
{
    /// <summary>
    /// Interaction logic for SignIn.xaml
    /// </summary>
    public partial class SignIn : Window
    {
        public SignIn()
        {
            InitializeComponent();
        }

        private async void SignIn_OnClosing(object sender, CancelEventArgs e)
        {
            if (Identity.Global.AccessToken == null)
            {
                PixevalEnvironment.LogoutExit = true;
                await Settings.Global.Store();
                Environment.Exit(0);
            }
        }

        private async void Login_OnClick(object sender, RoutedEventArgs e)
        {
            if (Email.Text.IsNullOrEmpty() || Password.Password.IsNullOrEmpty())
            {
                ErrorMessage.Content = Externally.EmptyEmailOrPasswordIsNotAllowed;
                return;
            }

            Login.Unable();

            try
            {
                await Authentication.Authenticate(Email.Text, Password.Password);
            }
            catch (Exception exception)
            {
                SetErrorHint(exception);
                Login.Enable();
                return;
            }

            var mainWindow = new MainWindow();
            mainWindow.Show();

            Close();
        }

        private async void SignIn_OnInitialized(object sender, EventArgs e)
        {
            if (Identity.ConfExists())
            {
                try
                {
                    DialogHost.IsOpen = true;
                    await Identity.RefreshIfRequired();
                }
                catch (ApiException exception)
                {
                    SetErrorHint(exception);

                    DialogHost.IsOpen = false;
                    return;
                }

                DialogHost.IsOpen = false;

                var mainWindow = new MainWindow();
                mainWindow.Show();
                Close();
            }
        }

        private async void SetErrorHint(Exception exception)
        {
            ErrorMessage.Content = exception is ApiException aException && await IsPasswordOrAccountError(aException)
                ? Externally.EmailOrPasswordIsWrong
                : exception.Message;
        }

        private static async ValueTask<bool> IsPasswordOrAccountError(ApiException exception)
        {
            var eMess = await exception.GetContentAsAsync<dynamic>();
            return eMess.errors.system.code == 1508;
        }
    }
}
