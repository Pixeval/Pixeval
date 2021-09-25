using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Globalization;
using JetBrains.Annotations;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.Win32;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Preference;
using Pixeval.Utilities;
using Pixeval.Messages;
using Pixeval.Misc;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;

namespace Pixeval.ViewModel
{
    public class LoginPageViewModel : INotifyPropertyChanged
    {
        // Remarks:
        // A Task that completes when the scan process of the Pixeval.LoginProxy completes
        // Note: the scan process consist of checksum matching and optionally file unzipping, see AppContext.CopyLoginProxyIfRequired()
        private static readonly TaskCompletionSource ScanLoginProxyTask = new();

        private static Process? _loginProxyProcess;

        static LoginPageViewModel()
        {
            WeakReferenceMessenger.Default.Register(RecipientAdapter<ScanningLoginProxyMessage>.Create(_ => ScanLoginProxyTask.SetResult()));
            // Kill the login proxy process if the application encounters exception
            WeakReferenceMessenger.Default.Register(RecipientAdapter<ApplicationExitingMessage>.Create(_ => _loginProxyProcess?.Kill()));
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
            [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseCheckingRefreshAvailable))]
            CheckingRefreshAvailable,

            [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseRefreshing))]
            Refreshing,

            [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseNegotiatingPort))]
            NegotiatingPort,

            [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseExecutingLoginProxy))]
            ExecutingLoginProxy,

            [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseCheckingCertificateInstallation))]
            CheckingCertificateInstallation,

            [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseInstallingCertificate))]
            InstallingCertificate,

            [LocalizedResource(typeof(LoginPageResources), nameof(LoginPageResources.LoginPhaseCheckingWebView2Installation))]
            CheckingWebView2Installation
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

        public bool CheckWebView2Installation()
        {
            AdvancePhase(LoginPhaseEnum.CheckingWebView2Installation);
            var regKey = Registry.LocalMachine.OpenSubKey(
                Environment.Is64BitOperatingSystem
                    ? @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}"
                    : @"SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}");
            return regKey != null && !(regKey.GetValue("pv") as string).IsNullOrEmpty();
        }

        public async Task<bool> CheckFakeRootCertificateInstallationAsync()
        {
            AdvancePhase(LoginPhaseEnum.CheckingCertificateInstallation);
            using var cert = await AppContext.GetFakeCaRootCertificateAsync();
            var fakeCertMgr = new CertificateManager(cert);
            return fakeCertMgr.Query(StoreName.Root, StoreLocation.CurrentUser);
        }

        public async Task InstallFakeRootCertificateAsync()
        {
            AdvancePhase(LoginPhaseEnum.InstallingCertificate);
            using var cert = await AppContext.GetFakeCaRootCertificateAsync();
            var fakeCertMgr = new CertificateManager(cert);
            fakeCertMgr.Install(StoreName.Root, StoreLocation.CurrentUser);
        }

        /// <summary>
        /// Check if the session file exists and satisfies the following four conditions: <br></br>
        /// 1. The <see cref="Session"/> object deserialized from the file is not null <br></br>
        /// 2. The <see cref="Session.RefreshToken"/> is not null <br></br>
        /// 3. The <see cref="Session.Cookie"/> is not null <br></br>
        /// 4. The <see cref="Session.CookieCreation"/> is within last seven days <br></br>
        /// </summary>
        /// <returns></returns>
        public bool CheckRefreshAvailable()
        {
            AdvancePhase(LoginPhaseEnum.CheckingRefreshAvailable);

            return AppContext.LoadSession() is { } session && CheckRefreshAvailableInternal(session);
        }

        private static bool CheckRefreshAvailableInternal(Session? session)
        {
            return session is not null && session.RefreshToken.IsNotNullOrEmpty() &&
                   session.Cookie.IsNotNullOrEmpty() && CookieNotExpired(session);

            static bool CookieNotExpired(Session session)
            {
                return DateTimeOffset.Now - session.CookieCreation <=
                       TimeSpan.FromDays(7); // check if the cookie is created within the last one week
            }
        }

        public async Task RefreshAsync()
        {
            AdvancePhase(LoginPhaseEnum.Refreshing);
            if (AppContext.LoadSession() is { } session && CheckRefreshAvailableInternal(session))
            {
                App.AppViewModel.MakoClient = new MakoClient(session, App.AppViewModel.AppSetting.ToMakoClientConfiguration(),
                    new RefreshTokenSessionUpdate());
                await App.AppViewModel.MakoClient.RefreshSessionAsync();
            }
            else
            {
                await MessageDialogBuilder.CreateAcknowledgement(
                        App.AppViewModel.Window,
                        LoginPageResources.RefreshingSessionIsNotPresentTitle,
                        LoginPageResources.RefreshingSessionIsNotPresentContent)
                    .ShowAsync();
                await AppContext.ClearAppLocalFolderAsync();
                Application.Current.Exit();
            }
        }

        public async Task WebLoginAsync()
        {
            // the web login requires another process to be created and started from this process
            // so check its presence before starts the login procedure
            await AppContext.CopyLoginProxyIfRequiredAsync();
            AdvancePhase(LoginPhaseEnum.NegotiatingPort);
            AdvancePhase(LoginPhaseEnum.ExecutingLoginProxy);
            await ScanLoginProxyTask.Task; // awaits the copy and extract operations to complete
            _loginProxyProcess = await CallLoginProxyAsync(ApplicationLanguages.ManifestLanguages[0]); // calls the login proxy process and passes the language and IPC server port
            var (cookie, code, verifier) = await WhenLoginTokenRequestedAsync(); // awaits the login proxy to sends the post request which contains the login result
            var session = await AuthCodeToSessionAsync(code, verifier, cookie);
            _loginProxyProcess = null; // if we reach here then the login procedure completes successfully, the login proxy process has been closed by itself, we do not need the control over it
            App.AppViewModel.MakoClient = new MakoClient(session, App.AppViewModel.AppSetting.ToMakoClientConfiguration(), new RefreshTokenSessionUpdate());
        }

        /// <summary>
        /// Starts the login proxy's executable. and passes the required parameters
        /// </summary>
        public static async Task<Process> CallLoginProxyAsync(string culture)
        {
            if (await AppContext.TryGetFileRelativeToLocalFolderAsync(Path.Combine(AppContext.AppLoginProxyFolder, "Pixeval.LoginProxy.exe")) is { } file)
            {
                return Process.Start(file.Path, culture);
            }

            throw new FileNotFoundException(MiscResources.CannotFindLoginProxyServerExecutable, "Pixeval.LoginProxy.exe");
        }

        [ContractAnnotation("=> halt")]
        public static async Task<(string cookie, string code, string verifier)> WhenLoginTokenRequestedAsync()
        {
            var pipeServer = new NamedPipeServerStream("pixiv_login");
            await pipeServer.WaitForConnectionAsync();
            var json = await JsonSerializer.DeserializeAsync<LoginTokenResponse>(pipeServer);
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
                pipeServer.Disconnect();
                return (cookie, token, verifier);
            }
            pipeServer.Disconnect();
            throw new LoginProxyException(LoginPageResources.LoginProxyConnectToHostFailed);
        }

        public static async Task<Session> AuthCodeToSessionAsync(string code, string verifier, string cookie)
        {
            // Remarks:
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
                ("redirect_uri", "https://app-api.pixiv.net/web/v1/users/auth/pixiv/callback"));
            result.EnsureSuccessStatusCode();
            var session = (await result.Content.ReadAsStringAsync()).FromJson<TokenResponse>()!.ToSession() with
            {
                Cookie = cookie,
                CookieCreation = DateTimeOffset.Now
            };
            return session;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}