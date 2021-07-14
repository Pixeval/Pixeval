using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Mako;
using Mako.Preference;
using Mako.Util;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Util;
using PropertyChanged;

namespace Pixeval.ViewModel
{
    public class LoginPageViewModel : INotifyPropertyChanged
    {
        public class LoginTokenResponse
        {
            [JsonPropertyName("errno")]
            public int Errno { get; set; }

            [JsonPropertyName("cookie")]
            public string? Cookie { get; set; }

            [JsonPropertyName("code")]
            public string? Code { get; set; }
        }

        public enum LoginPhaseEnum
        {
            [Metadata(LoginPageResources.LoginPhaseCheckingRefreshAvailable)]
            CheckingRefreshAvailable,

            [Metadata(LoginPageResources.LoginPhaseRefreshing)]
            Refreshing,

            [Metadata(LoginPageResources.LoginPhaseNegotiatingPort)]
            NegotiatingPort,

            [Metadata(LoginPageResources.LoginPhaseExecutingLoginProxy)]
            ExecutingLoginProxy
        }

        private LoginPhaseEnum _loginPhase;

        public LoginPhaseEnum LoginPhase
        {
            get => _loginPhase;
            set
            {
                if (value == _loginPhase) return;
                _loginPhase = value;
                OnPropertyChanged();
            }
        }

        public void AdvancePhase(LoginPhaseEnum newPhase)
        {
            LoginPhase = newPhase;
        }

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
            return session is not null && session.RefreshToken.IsNotNullOrEmpty() && session.Cookie.IsNotNullOrEmpty() && CookieNotExpired(session);
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
                    .WithTitle(LoginPageResources.RefreshingSessionIsNotPresentTitle)
                    .WithContent(LoginPageResources.RefreshingSessionIsNotPresentContent)
                    .WithPrimaryButtonText(MessageContentDialogResources.OkButtonContent)
                    .WithDefaultButton(ContentDialogButton.Primary)
                    .Build(App.Window)
                    .ShowAsync();
                await AppContext.AppLocalFolder.ClearDirectoryAsync();
                Application.Current.Exit();
            }
        }

        public static async void CallLoginProxy(string culture, int port)
        {
            if (await AppContext.TryGetFileRelativeToLocalFolderAsync(Path.Combine(AppContext.AppLoginProxyFolder, "Pixeval.LoginProxy.exe")) is { } file)
            {
                Process.Start(file.Path, $"{port} {culture}");
            }
            else throw new FileNotFoundException(MiscResources.CannotFindLoginProxyServerExecutable, "Pixeval.LoginProxy.exe");
        }

        public static async Task<(string cookie, string token)> WhenLoginTokenRequestedAsync(int port)
        {
            // shit code
            var httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://localhost:{port}/");
            httpListener.Start();
            while (true)
            {
                var context = await httpListener.GetContextAsync();
                if (context.Request.Url?.PathAndQuery is "/login/token"
                    && context.Request.HasEntityBody
                    && context.Request.HttpMethod == "POST")
                {
                    using var streamReader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding, leaveOpen: false);
                    var content = await streamReader.ReadToEndAsync();
                    var json = content.FromJson<LoginTokenResponse>();
                    if (json?.Cookie is { } cookie && json.Code is { } token)
                    {
                        SendResponse(context, 200);
                        return (cookie, token);
                    }
                    SendResponse(context, 400);
                }
            }

            static void SendResponse(HttpListenerContext context, int code)
            {
                context.Response.StatusCode = code;
                context.Response.SendChunked = false;
                context.Response.Headers.Clear();
                context.Response.Close();
            }
        }

        private static async Task<MakoClientConfiguration?> LoadMakoClientConfiguration()
        {
            return await AppContext.TryGetFileRelativeToLocalFolderAsync(AppContext.AppConfigurationFileName) is { } file ? (await file.ReadStringAsync()).FromJson<MakoClientConfiguration>() : null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}