using System;
using System.IO;
using System.Threading.Tasks;
using Mako;
using Mako.Preference;
using Mako.Util;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Util;
using PropertyChanged;

namespace Pixeval.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class LoginPageViewModel
    {
        public bool RefreshAvailable { get; set; }

        public async Task<bool> CheckRefreshAvailable()
        {
            static bool CookieNotExpired(Session session)
            {
                return DateTime.Now - session.CookieCreation <= TimeSpan.FromDays(7); // check if the cookie is created within the last one week
            }

            if (!File.Exists(AppContext.AppConfigurationFileName))
            {
                return false;
            }

            var session = (await File.ReadAllTextAsync(AppContext.AppSessionFileName)).FromJson<Session>();
            return session is not null && session.RefreshToken.IsNotNullOrEmpty() && CookieNotExpired(session);
        }

        public async Task Refresh()
        {
            if ((await File.ReadAllTextAsync(AppContext.AppSessionFileName)).FromJson<Session>() is { } session && session.RefreshToken.IsNotNullOrEmpty() && session.Cookie.IsNotNullOrEmpty())
            {
                App.PixevalAppClient = new MakoClient(session, await LoadMakoClientConfiguration() ?? new MakoClientConfiguration(), new RefreshTokenSessionUpdate());
                await App.PixevalAppClient.RefreshSessionAsync();
            }
            else
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
            }
        }

        private static async Task<MakoClientConfiguration?> LoadMakoClientConfiguration()
        {
            return (await File.ReadAllTextAsync(AppContext.AppConfigurationFileName)).FromJson<MakoClientConfiguration>();
        }
    }
}