using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Pixeval.Objects;
using Pixeval.Objects.I18n;
using Pixeval.Persisting;
using Pixeval.UI.UserControls;

namespace Pixeval.UI
{
    public partial class SessionRefreshing
    {
        public bool Refreshing { get; }

        private readonly string token;
        private readonly string codeVerifier;
        private readonly string cookie;

        private readonly TaskCompletionSource<Exception> loginCompletion = new TaskCompletionSource<Exception>();

        public SessionRefreshing(string token, string cookie, string codeVerifier = null, bool refreshing = false)
        {
            (Refreshing, this.token, this.cookie, this.codeVerifier) = (refreshing, token, cookie, codeVerifier);
            InitializeComponent();
            HintMessageTextBlock.Text = Refreshing ? AkaI18N.SignInUpdatingSession : AkaI18N.SignInLoggingIn;
            loginCompletion.Task.ContinueWith(async task =>
            {
                var exception = await task;
                if (exception == null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        new MainWindow().Show();
                        Close();
                    });
                }
                else if (await Dispatcher.Invoke(async () => await MessageDialog.Warning(MessageDialogHost, exception.Message)) == Objects.DialogResult.Yes)
                {
                    Environment.Exit(-1);
                }
            });
        }

        private async void SessionRefreshing_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Refreshing)
                {
                    await Authentication.Refresh(token);
                }
                else
                {
                    await Authentication.AuthorizationCodeToToken(token, codeVerifier);
                }
                Session.Current.Cookie = cookie;
            }
            catch (Exception exception)
            {
                loginCompletion.TrySetResult(exception);
                return;
            }
            loginCompletion.TrySetResult(null);
        }

        private void SessionRefreshing_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
