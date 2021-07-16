using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Globalization;
using JetBrains.Annotations;
using Mako;
using Mako.Model;
using Mako.Net;
using Mako.Preference;
using Mako.Util;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Events;
using Pixeval.Util;

namespace Pixeval.ViewModel
{
    public class LoginPageViewModel : INotifyPropertyChanged
    {
        private static readonly Task ScanLoginProxyTask;

        private static Process? _loginProxyProcess;

        static LoginPageViewModel()
        {
            ScanLoginProxyTask = App.PixevalEventChannel.SubscribeTask<ScanningLoginProxyEvent>((evt, source) =>
            {
                evt.ScanTask.ContinueWith(_ => source.SetResult());
            });
        }

        public class LoginTokenResponse
        {
            [JsonPropertyName("errno")]
            public int Errno { get; set; }

            [JsonPropertyName("cookie")]
            public string? Cookie { get; set; }

            [JsonPropertyName("code")]
            public string? Code { get; set; }

            [JsonPropertyName("verifier")]
            public string? Verifier { get; set; }
        }

        public enum LoginPhaseEnum
        {
            [Metadata(nameof(LoginPageResources.LoginPhaseCheckingRefreshAvailable))]
            CheckingRefreshAvailable,

            [Metadata(nameof(LoginPageResources.LoginPhaseRefreshing))]
            Refreshing,

            [Metadata(nameof(LoginPageResources.LoginPhaseNegotiatingPort))]
            NegotiatingPort,

            [Metadata(nameof(LoginPageResources.LoginPhaseExecutingLoginProxy))]
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
            AdvancePhase(LoginPhaseEnum.CheckingRefreshAvailable);

            if (!File.Exists(AppContext.AppConfigurationFileName))
            {
                return false;
            }

            var session = (await File.ReadAllTextAsync(AppContext.AppSessionFileName)).FromJson<Session>();
            return session is not null && session.RefreshToken.IsNotNullOrEmpty() && session.Cookie.IsNotNullOrEmpty() && CookieNotExpired(session);

            static bool CookieNotExpired(Session session)
            {
                return DateTime.Now - session.CookieCreation <= TimeSpan.FromDays(7); // check if the cookie is created within the last one week
            }
        }

        public async Task Refresh()
        {
            AdvancePhase(LoginPhaseEnum.Refreshing);
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

        public async Task WebLogin()
        {
            await AppContext.CopyLoginProxyIfRequired();
            AdvancePhase(LoginPhaseEnum.NegotiatingPort);
            var port = IOHelper.NegotiatePort();
            AdvancePhase(LoginPhaseEnum.ExecutingLoginProxy);
            await ScanLoginProxyTask;
            _loginProxyProcess = await CallLoginProxy(ApplicationLanguages.ManifestLanguages[0], port);
            var (cookie, code, verifier) = await WhenLoginTokenRequestedAsync(port);
            var session = await AuthCodeToSession(code, verifier, cookie);
            _loginProxyProcess = null;
            App.PixevalAppClient = new MakoClient(session, await LoadMakoClientConfiguration() ?? new MakoClientConfiguration(), new RefreshTokenSessionUpdate());
        }

        public static string GetLoginPhaseString(LoginPhaseEnum loginPhase)
        {
            return (string) typeof(LoginPageResources).GetField(loginPhase.GetMetadataOnEnumMember()!)?.GetValue(null)!;
        }

        public static async Task<Process> CallLoginProxy(string culture, int port)
        {
            if (await AppContext.TryGetFileRelativeToLocalFolderAsync(Path.Combine(AppContext.AppLoginProxyFolder, "Pixeval.LoginProxy.exe")) is { } file)
            {
                return Process.Start(file.Path, $"{port} {culture}");
            }

            throw new FileNotFoundException(MiscResources.CannotFindLoginProxyServerExecutable, "Pixeval.LoginProxy.exe");
        }

        public static async Task<(string cookie, string code, string verifier)> WhenLoginTokenRequestedAsync(int port)
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
                    if (json?.Errno is { } and not 0)
                    {
                        throw new LoginProxyException(json.Errno switch
                        {
                            1 => LoginPageResources.LoginProxyConnectToHostFailed,
                            2 => LoginPageResources.LoginProxyCannotFindCertificate,
                            _ => MiscResources.UnexpectedBehavior
                        });
                    }
                    if (json?.Cookie is { } cookie && json.Code is { } token && json.Verifier is { } verifier)
                    {
                        SendResponse(context, 200);
                        return (cookie, token, verifier);
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

        public static async Task<Session> AuthCodeToSession(string code, string verifier, string cookie)
        {
            // HttpClient is designed to be used through whole application lifetime, create and
            // dispose it in a function is a commonly misused anti-pattern, but this function
            // is intended to be called only once (at the start time) during the entire application's
            // lifetime, so the overhead is acceptable
            using var httpClient = new HttpClient(new DelegatedHttpMessageHandler(MakoHttpOptions.CreateHttpMessageInvoker(new PixivApiNameResolver())));
            httpClient.DefaultRequestHeaders.Add("User-Agent", "PixivAndroidApp/5.0.64 (Android 6.0)");
            var result = await httpClient.PostFormAsync("http://oauth.secure.pixiv.net/auth/token",
                ("code", code),
                ("code_verifier", verifier),
                ("client_id", "MOBrBDS8blbauoSck0ZfDbtuzpyT"),
                ("client_secret", "lsACyCD94FhDUtGTXi3QzcFE2uU1hqtDaKeqrdwj"),
                ("grant_type", "authorization_code"),
                ("include_policy", "true"),
                ("redirect_uri", "https://app-api.pixiv.net/web/v1/users/auth/pixiv/callback")
            );
            result.EnsureSuccessStatusCode();
            return (await result.Content.ReadAsStringAsync()).FromJson<TokenResponse>()!.ToSession() with
            {
                Cookie = cookie
            };
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